
using Travel.Core.Domain.Entities.Enums;
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class VehicleMileage : FullAuditedEntity<Guid>
    {
        public string VehicleRegistrationNumber { get; set; }
        public ServiceLevelType ServiceLevel { get; set; }

        public int CurrentMileage { get; set; }
        public DateTime? LastServiceDate { get; set; }

        /// <summary>
        /// Date the vehicle mileage reached the current service level
        /// </summary>
        public DateTime? DateDue { get; set; }
        public bool IsDue { get; set; }
        public bool IsDeactivated { get; set; }
        
        public NotificationLevel NotificationLevel { get; set; }
    }
}
