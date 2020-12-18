using Travel.Core.Common.Enums;
using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class VehicleTripRegistrationDTO
    {
        public Guid Id { get; set; }
        public Guid? VehicleTripRegistrationId { get; set; }
        public string PhysicalBusRegistrationNumber { get; set; }
        public DateTime DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string RouteName { get; set; }
        public bool IsVirtualBus { get; set; }
        public bool IsBusFull { get; set; }
        public bool IsBlownBus { get; set; }
        public bool ManifestPrinted { get; set; }
        public int? RouteId { get; set; }
        public decimal DriverFee { get; set; }
        // FK
        public string DriverCode { get; set; }
        public string OriginalCaptainCode { get; set; }

        public int? BookingTypeId { get; set; }
        public virtual JourneyType JourneyType { get; set; }

        public Guid TripId { get; set; }
        public int? CurrentModelId { get; set; }
        public int? VehicleModelId { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleCaptaindetails { get; set; }
        public int? OriginalModelId { get; set; }
        public JourneyStatus JourneyStatus { get; set; }
        public long? BookingId { get; set; }

        //
        public TripType TripType { get; set; }
        public byte SeatNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
