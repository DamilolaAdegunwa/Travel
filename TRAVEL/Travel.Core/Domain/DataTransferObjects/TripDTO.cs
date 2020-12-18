using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class TripDTO
    {
        public Guid Id { get; set; }
        public string DepartureTime { get; set; }
        public string DepartureDate { get; set; }
        public string ParentDepartureTime { get; set; }

        public string Code { get; set; }
        public string JourneyStatus { get; set; }
        public string DriverCode { get; set; }
        public string VehicleTripRegId { get; set; }
        public int RouteId { get; set; }
        public string RouteName { get; set; }
        public int? ParentRouteId { get; set; }
        public string ParentRouteName { get; set; }
        public Guid? ParentTripId { get; set; }
        public int? VehicleModelId { get; set; }
        public bool AvailableOnline { get; set; }
        public string VehicleModelName { get; set; }
    }
}