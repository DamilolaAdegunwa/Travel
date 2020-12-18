
using Travel.Core.Domain.Entities.Enums;
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class VehicleTripRegistration : FullAuditedEntity<Guid>
    {
        public string PhysicalBusRegistrationNumber { get; set; }
        public DateTime DepartureDate { get; set; }

        public bool IsVirtualBus { get; set; }
        public bool IsBusFull { get; set; }
        public bool IsBlownBus { get; set; }
        public bool ManifestPrinted { get; set; }
        public string DriverCode { get; set; }
        public string OriginalDriverCode { get; set; }

        public int? BookingTypeId { get; set; }
        public virtual BookingType BookingType { get; set; }

        public virtual JourneyType JourneyType { get; set; }

        public Guid TripId { get; set; }
        public virtual Trip Trip { get; set; }

        public int? VehicleModelId { get; set; }
        public virtual VehicleModel VehicleModel { get; set; }
    }
}