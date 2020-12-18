using Travel.Core.Domain.Entities;

using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class Manifest:FullAuditedEntity<Guid>
    {
        public int NumberOfSeats { get; set; }
        public bool IsPrinted { get; set; }

        public decimal? Amount { get; set; }
        public decimal? Dispatch { get; set; }
        public DateTime? ManifestPrintedTime { get; set; }
        public Guid VehicleTripRegistrationId { get; set; }
        public virtual VehicleTripRegistration VehicleTripRegistration { get; set; }

        public int? VehicleModelId { get; set; }
        public virtual VehicleModel VehicleModel { get; set; }

        public string Employee { get; set; }
        public string DispatchSource { get; set; }
    }
}