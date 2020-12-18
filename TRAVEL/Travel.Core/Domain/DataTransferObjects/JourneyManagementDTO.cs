using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class JourneyManagementDTO
    {
        public Guid Id { get; set; }
        public DateTime? ActualTripStartTime { get; set; }
        public DateTime? TripStartTime { get; set; }
        public DateTime? TripEndTime { get; set; }
        public int? TransloadedJourneyId { get; set; }
        public DateTime JourneyDate { get; set; }
        public string ApprovedBy { get; set; }
        public string ReceivedBy { get; set; }
        public decimal DispatchFee { get; set; }
        public decimal DriverFee { get; set; }
        public decimal LoaderFee { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
        public string Route { get; set; }
        public string VehicleNo { get; set; }
        public string Driver { get; set; }
        // FK
        public Guid VehicleTripRegistrationId { get; set; }
        public string DenialReason { get; set; }
        public JourneyStatus JourneyStatus { get; set; }
        public JourneyType JourneyType { get; set; }
        public string DriverName { get; set; }
        public int Shortage { get; set; }
        public DateTime VehicleDate { get; set; }
        public string VehicleModel { get; set; }

        public double AverageRating { get; set; }
        public string OriginalCaptain { get; set; }
        public string OriginalCaptainName { get; set; }
        public string Ebm { get; set; }

        // Extended Properties
        public int DepartureTerminalId { get; set; }
        public int DestinationTerminalId { get; set; }
        public string DepartureTime { get; set; }
        public int Capacity { get; set; }
        public DriverTripStatus DriverTripStatus { get; set; }
    }
}