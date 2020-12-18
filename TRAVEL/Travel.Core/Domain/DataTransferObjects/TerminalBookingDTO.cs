using Travel.Core.Common.Enums;
using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class TerminalBookingDTO
    {
        public TripType TripType { get; set; }
        public Guid TripId { get; set; }
        public int RouteId { get; set; }
        public string RouteName { get; set; }
        public string DepartureTime { get; set; }

        public int DestinationTerminalId { get; set; }
        public int DepartureTerminalId { get; set; }

        public DateTime? DepartureDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public int NumberOfAdults { get; set; }
        public int NumberOfChildren { get; set; }
        public int VehicleModelId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleRegistrationNumber { get; set; }
        public string DriverCode { get; set; }
        public string OriginalDriverCode { get; set; }
    }
}