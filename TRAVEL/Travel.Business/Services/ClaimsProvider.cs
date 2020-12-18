using Travel.Core.Utils;
using System.Collections.Generic;
using System.Security.Claims;

namespace Travel.Business.Services
{
    public class PermissionClaim : Claim
    {
        public PermissionClaim(string value) : base("Permission", value)
        {
        }
    }

    public class PermissionClaimsProvider
    {
        public static readonly PermissionClaim Dashboard = new PermissionClaim("dashboard");
        public static readonly PermissionClaim ManageEmployee = new PermissionClaim("manageemployee");
        public static readonly PermissionClaim ManageCustomer = new PermissionClaim("managecustomer");
        public static readonly PermissionClaim ManageRoute = new PermissionClaim("manageroute");
        public static readonly PermissionClaim ManageVehicleModel = new PermissionClaim("managevehiclemodel");
        public static readonly PermissionClaim ManageVehicle = new PermissionClaim("managevehicle");
        public static readonly PermissionClaim ManageVehicleMake = new PermissionClaim("managevehiclemake");
        public static readonly PermissionClaim ManageTerminal = new PermissionClaim("manageterminal");
        public static readonly PermissionClaim ManageTrip = new PermissionClaim("managetrip");
        public static readonly PermissionClaim ManageReport = new PermissionClaim("managereport");
        public static readonly PermissionClaim ManageDriver = new PermissionClaim("managedriver");
        public static readonly PermissionClaim ManageHireBooking = new PermissionClaim("managehirebooking");
        public static readonly PermissionClaim ManageRole = new PermissionClaim("managerole");
        public static readonly PermissionClaim ManageTerminalBooking = new PermissionClaim("manageterminalbooking");
        public static readonly PermissionClaim ManageAdvancedBooking = new PermissionClaim("manageadvancedbooking");
        public static readonly PermissionClaim ManageState = new PermissionClaim("managestate");
        public static readonly PermissionClaim ManageRegion = new PermissionClaim("manageregion");
        public static readonly PermissionClaim ManageFare = new PermissionClaim("managefare");
        public static readonly PermissionClaim managevehicleallocation = new PermissionClaim("managevehicleallocation");
        

        public static Dictionary<string, IEnumerable<PermissionClaim>> GetSystemDefaultRoles()
        {
            return new Dictionary<string, IEnumerable<PermissionClaim>>
            {
                    {    CoreConstants.Roles.Admin, new PermissionClaim []{
                                        Dashboard ,
                                        ManageEmployee,
                                        ManageCustomer ,
                                        ManageRoute,
                                        ManageVehicleModel,
                                        ManageVehicle ,
                                        ManageVehicleMake,
                                        ManageTerminal ,
                                        ManageTrip ,
                                        ManageReport,
                                        ManageDriver,
                                        ManageHireBooking,
                                        ManageRole,
                                        ManageTerminalBooking,
                                        ManageAdvancedBooking,
                                        ManageState ,
                                        ManageRegion,
                                        ManageFare
                         }
                    },

                    {    CoreConstants.Roles.OM, new PermissionClaim []{
                           ManageAdvancedBooking,
                           ManageTerminalBooking,
                           managevehicleallocation
                         }
                    },
                    {    CoreConstants.Roles.BM, new PermissionClaim []{
                            ManageRoute,
                            ManageTrip,
                            ManageFare,
                            ManageDriver,
                            ManageVehicle
                    }
                    },
                    {    CoreConstants.Roles.T, new PermissionClaim []{
                           ManageAdvancedBooking,
                           ManageTerminalBooking
                         }
                    },
                    {    CoreConstants.Roles.A, new PermissionClaim []{
                           ManageReport
                         }
                    },
                     {    CoreConstants.Roles.AC, new PermissionClaim []{
                         }
                    },
                    {    CoreConstants.Roles.TM, new PermissionClaim []{
                         }
                    },
                    {    CoreConstants.Roles.CC, new PermissionClaim []{
                         }
                    }
            };
        }

        public static IEnumerable<PermissionClaim> GetClaims()
        {
            return new PermissionClaim[] {
                Dashboard ,
                ManageEmployee,
                ManageCustomer ,
                ManageRoute,
                ManageVehicleModel,
                ManageVehicle ,
                ManageVehicleMake,
                ManageTerminal ,
                ManageTrip ,
                ManageReport,
                ManageDriver,
                ManageHireBooking,
                ManageRole,
                ManageTerminalBooking,
                ManageAdvancedBooking,
                ManageState ,
                ManageRegion,
                ManageFare,
                managevehicleallocation
            };
        }
    }
}