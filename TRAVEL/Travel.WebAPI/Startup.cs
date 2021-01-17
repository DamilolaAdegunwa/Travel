using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Travel.Business.Services;
using Travel.Core.Caching;
using Travel.Core.Collections.Extensions;
using Travel.Core.Configuration;
using Travel.Core.Domain.Entities;
using Travel.Core.Messaging.Email;
using Travel.Core.Messaging.Sms;
using Travel.Core.Utils;
using Travel.Data.efCore;
using Travel.Data.efCore.Context;
using Travel.Data.UnitOfWork;
using Travel.WebAPI.Infrastructure.Services;
using Travel.WebAPI.Models;
using Travel.WebAPI.Utils;
using Travel.WebAPI.Utils.Extentions;
using Microsoft.OpenApi.Models;
namespace Travel.WebAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        //public Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment { get; }
        public IHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            #region Identity

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString(WebConstants.ConnectionStringName))
                , ServiceLifetime.Transient);

            services.AddIdentity<User, Role>(options => {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Lockout.AllowedForNewUsers = false;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            #endregion

            #region Services
            services.AddTransient<DbContext>((_) => {
                var connStr = Configuration.GetConnectionString(WebConstants.ConnectionStringName);
                return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                         .UseSqlServer(connStr)
                                         .Options);
            });

            services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
            services.AddScoped(typeof(IDbContextProvider<>), typeof(UnitOfWorkDbContextProvider<>));
            services.RegisterGenericRepos(typeof(ApplicationDbContext));

            services.AddScoped<IErrorCodeService, ErrorCodeService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IWalletNumberService, WalletNumberService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IReferralService, ReferralService>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<IRegionService, RegionService>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<ITerminalService, TerminalService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IVehicleModelService, VehicleModelService>();
            services.AddScoped<IVehicleMakeService, VehicleMakeService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IManifestService, ManifestService>();
            services.AddScoped<ISeatManagementService, SeatManagementService>();
            services.AddScoped<ITripService, TripService>();
            services.AddScoped<IDiscountService, DiscountService>();
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IMtuReports, MtuReportService>();
            services.AddScoped<ITripAvailabilityService, TripAvailabilityService>();
            services.AddScoped<IPickupPointService, PickupPointService>();
            services.AddScoped<IAccountTransactionService, AccountTransactionService>();
            services.AddScoped<IFareService, FareService>();
            services.AddScoped<IFareCalendarService, FareCalendarService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IVehicleTripRegistrationService, VehicleTripRegistrationService>();
            services.AddScoped<IAccountSummaryService, AccountSummaryService>();
            services.AddScoped<IHireRequestService, HireRequestService>();
            services.AddScoped<IBookingReportService, BookingReportService>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<ISubRouteService, SubRouteService>();
            services.AddScoped<IJourneyManagementService, JourneyManagementService>();
            services.AddScoped<IManifestService, ManifestService>();
            services.AddScoped<IFranchizeService, FranchizeService>();
            services.AddScoped<IPassportTypeService, PassportTypeService>();

            #endregion

            services.Configure<JwtConfig>(options =>
                        Configuration.GetSection(WebConstants.Sections.AuthJwtBearer).Bind(options));

            services.Configure<BookingConfig>(options =>
                       Configuration.GetSection(WebConstants.Sections.Booking).Bind(options));

            services.Configure<AppConfig>(options =>
                     Configuration.GetSection(WebConstants.Sections.App).Bind(options));

            services.Configure<SmtpConfig>(options =>
                     Configuration.GetSection(WebConstants.Sections.Smtp).Bind(options));

            services.Configure<PaymentConfig.Paystack>(options =>
                     Configuration.GetSection(WebConstants.Sections.Paystack).Bind(options));

            services.Configure<DataProtectionTokenProviderOptions>(o =>
                     o.TokenLifespan = TimeSpan.FromHours(3));

            #region Auth
            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x => {
                var jwtConfig = new JwtConfig();

                Configuration.Bind(WebConstants.Sections.AuthJwtBearer, jwtConfig);

                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.FromMinutes(3),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig.SecurityKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidateLifetime = true,
                    ValidateAudience = false
                };
                x.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context => {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            //#endregion

            //#region Auth
            services.AddAuthorization(options => {
                SetupPolicies(options);
            });
            services.AddCors();
            services.AddDistributedMemoryCache();

            #endregion

            services.AddHttpContextAccessor();
            services.AddTransient<IServiceHelper, ServiceHelper>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<ICacheManager, MemoryCacheManager>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IMailService, SmtpEmailService>();
            services.AddTransient<ISMSService, SMSService>();
            services.AddTransient<IWebClient, WebClient>();
            services.AddSingleton<IGuidGenerator>((s) => SequentialGuidGenerator.Instance);

            //if (Environment.IsDevelopment()) {

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo /*Info*/ { Title = "LME Web API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme() /*ApiKeyScheme()*/
                {
                    
                    Description = "Token Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,//"header",
                    Type = SecuritySchemeType.ApiKey, //"apiKey",
                });

                //options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                //      {
                //        { JwtBearerDefaults.AuthenticationScheme, new string[] { } }
                //      });
                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement()
                    {
                        
                    }
                );
            });

            //}

            services.AddMvc()
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            //.AddNewtonsoftJson()
            //.AddJsonOptions(o => {

            //    o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            //});


            services.AddControllers()//.AddNewtonsoftJson();
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });
        }

        private static void SetupPolicies(Microsoft.AspNetCore.Authorization.AuthorizationOptions options)
        {
            options.AddPolicy("Manage Customer", policy =>
                 policy.RequireClaim("Permission", PermissionClaimsProvider.ManageCustomer.Value));

            options.AddPolicy("Manage Employee", policy =>
                 policy.RequireClaim("Permission", PermissionClaimsProvider.ManageEmployee.Value));

            options.AddPolicy("Manage Report", policy =>
                policy.RequireClaim("Permission", PermissionClaimsProvider.ManageReport.Value));

            options.AddPolicy("Manage State", policy =>
                policy.RequireClaim("Permission", PermissionClaimsProvider.ManageState.Value));

            options.AddPolicy("Manage Region", policy =>
                policy.RequireClaim("Permission", PermissionClaimsProvider.ManageRegion.Value));

            options.AddPolicy("Manage HireBooking", policy =>
                policy.RequireClaim("Permission", PermissionClaimsProvider.ManageHireBooking.Value));

            options.AddPolicy("Manage Vehicle", policy =>
                policy.RequireClaim("Permission", PermissionClaimsProvider.ManageVehicle.Value));

            options.AddPolicy("Manage Terminal", policy =>
              policy.RequireClaim("Permission", PermissionClaimsProvider.ManageTerminal.Value));

            options.AddPolicy("Manage Route", policy =>
              policy.RequireClaim("Permission", PermissionClaimsProvider.ManageRoute.Value));

            options.AddPolicy("Manage Trip", policy =>
              policy.RequireClaim("Permission", PermissionClaimsProvider.ManageTrip.Value));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (Environment.IsDevelopment())
            {
                //UserSeed.SeedDatabase(app);
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseCors(x => {
                x.WithOrigins(Configuration["App:CorsOrigins"]
                  .Split(",", StringSplitOptions.RemoveEmptyEntries)
                  .Select(o => o.RemovePostFix("/"))
                  .ToArray())
             .AllowAnyMethod()
             .AllowAnyHeader();
            });
            app.UseAuthentication();
            //app.UseStatusCodePages();
            //app.UseMvc();

            // if (Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI(options => {
                options.SwaggerEndpoint(Configuration["App:ServerRootAddress"].EnsureEndsWith('/') + "swagger/v1/swagger.json", "Travel.Web API V1");
            });
            // }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
