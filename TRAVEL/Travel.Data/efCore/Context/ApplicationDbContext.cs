using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Context
{
    /// <Note>
    /// DbSet properties are being used by generic repository
    /// </Note>
    public class ApplicationDbContext : BaseDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeRoute> EmployeeRoutes { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<SubRoute> SubRoutes { get; set; }
        public DbSet<TripSetting> TripSettings { get; set; }
        public DbSet<Driver> Drivers { get; set; }
       
        public DbSet<DriverAccount> DriverAccounts { get; set; }
        public DbSet<Fare> Fares { get; set; }
        public DbSet<FareCalendar> FareCalendars { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Terminal> Terminals { get; set; }
        public DbSet<VehicleTripRegistration> VehicleTripRegistrations { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<VehicleMake> VehicleMakes { get; set; }
        public DbSet<VehicleMileage> VehicleMileages { get; set; }
        public DbSet<ExcludedSeat> ExcludedSeats { get; set; }
        public DbSet<VehiclePart> VehicleParts { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehiclePartPosition> VehiclePartPositions { get; set; }
        public DbSet<SeatManagement> SeatManagements { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletNumber> WalletNumbers { get; set; }
        public DbSet<JourneyManagement> JourneyManagements { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<HireRequest> HireRequests { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<ErrorCode> ErrorCodes { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<AccountSummary> AccountSummaries { get; set; }
        public DbSet<AccountTransaction> AccountTransactions { get; set; }
        public DbSet<TripAvailability> TripAvailabilities { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<PickupPoint> PickupPoints { get; set; }
        public DbSet<Manifest> ManifestManagements { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<PayStackWebhookResponse> PayStackWebhookResponses { get; set; }
        public DbSet<FranchiseUser> FranchiseUsers { get; set; }
        public DbSet<Franchize> Franchize { get; set; }
        public DbSet<PassportType> PassportType { get; set; }
        public DbSet<Franchize> Franchizee { get; set; }
        public DbSet<MtuReportModel> MTU { get; set; }
        public DbSet<MtuPhoto> MtuPhoto { get; set; }
        public DbSet<VehicleAllocationDetailModel> VehicleAllocationDetail { get; set; }
        //test

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }

    /// <summary>
    /// Migration only
    /// </summary>
    public class AppDbContextMigrationFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public static readonly IConfigurationRoot ConfigBuilder = new ConfigurationBuilder()
                 .SetBasePath(AppContext.BaseDirectory)
                 .AddJsonFile("appsettings.json", false, true).Build();

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                   .UseSqlServer(ConfigBuilder.GetConnectionString("Database"))
                                   .Options);
        }
    }
}