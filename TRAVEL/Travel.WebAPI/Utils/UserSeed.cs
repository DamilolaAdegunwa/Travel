using Travel.Business.Services;
using Travel.Core.Domain.Entities;
using Travel.Core.Utils;
using Travel.Data.efCore.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Travel.WebAPI.Utils
{
    public class UserSeed
    {
        public static void SeedDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope()) {

                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();

                CreateDefaultRolesAndPermissions(scope);
                CreateAdminAccount(scope);
                CreateOnlineBookingAccounts(scope);
            }

            Console.WriteLine("Done seeding database.");
        }

        static void CreateAdminAccount(IServiceScope serviceScope)
        {

            var userMgr = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var admin = new User
            {
                UserName = WebConstants.DefaultAccount,
                Email = WebConstants.DefaultAccount,
                EmailConfirmed = true
            };


            if (userMgr.FindByNameAsync(admin.UserName).Result is null) {

                var result = userMgr.CreateAsync(admin, "Lme@adm1n").Result;

                if (result.Succeeded) {

                    var adminRole = GetRole(serviceScope, CoreConstants.Roles.Admin);

                    if (adminRole != null) {
                        var adminRoleResult = userMgr.AddToRoleAsync(admin, adminRole.Name).Result;
                    }
                }
            }
        }

        static void CreateOnlineBookingAccounts(IServiceScope serviceScope)
        {
            var userMgr = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var bookingAccts = new User[]{
                    new User{
                    UserName = WebConstants.WebBookingAccount,
                    Email = WebConstants.WebBookingAccount,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed=true
                },
                new User{
                    UserName = WebConstants.IosBookingAccount,
                    Email = WebConstants.IosBookingAccount,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed=true
                },
                new User{
                    UserName = WebConstants.AndroidBookingAccount,
                    Email = WebConstants.AndroidBookingAccount,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed=true
                }
            };

            Array.ForEach(bookingAccts, bookingAcct => {
                if (userMgr.FindByNameAsync(bookingAcct.UserName).Result is null) {

                    var result = userMgr.CreateAsync(bookingAcct, "Lme@onl1n3").Result;
                }
            });
        }

        static Role GetRole(IServiceScope serviceScope, string role)
        {
            var roleMgr = serviceScope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            return roleMgr.FindByNameAsync(role).Result;
        }

        public static void CreateDefaultRolesAndPermissions(IServiceScope serviceScope)
        {
            var roleMgr = serviceScope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var systemRoles = PermissionClaimsProvider.GetSystemDefaultRoles();

            if (systemRoles is null || !systemRoles.Any())
                return;

            foreach (var systemClaims in systemRoles) {

                var role = roleMgr.FindByNameAsync(systemClaims.Key).Result;

                if (role is null) {

                    role = new Role
                    {
                        Name = systemClaims.Key,
                        IsActive = true,
                        IsDefaultRole = true,
                    };

                    var r = roleMgr.CreateAsync(role).Result;
                }

                var oldClaims = roleMgr.GetClaimsAsync(role).Result;

                foreach (var claim in systemClaims.Value) {
                    if (!oldClaims.Any(x => x.Value.Equals(claim.Value))) {

                        var r = roleMgr.AddClaimAsync(role, claim).Result;
                    }
                }
            }
        }
    }
}