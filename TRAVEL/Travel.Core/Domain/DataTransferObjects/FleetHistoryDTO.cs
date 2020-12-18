using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class FleetHistoryDTO
    {
        public int Id { get; set; }
        public string DriverCode { get; set; }
        public string DriverName { get; set; }
        public Guid? VehicleTripRegistrationId { get; set; }
        public string PhysicalBusRegistrationNumber { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int? RouteId { get; set; }
        public string RouteName { get; set; }
        public virtual JourneyType JourneyType { get; set; }
        public string JourneyTypes { get; set; }
        public Guid TripId { get; set; }
        public JourneyStatus JourneyStatus { get; set; }
        public string JourneyStatuses { get; set; }
        public bool IsPrinted { get; set; }
    }
}
