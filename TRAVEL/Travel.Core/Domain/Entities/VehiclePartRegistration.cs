
using Travel.Core.Domain.Entities.Enums;
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class VehiclePartRegistration : FullAuditedEntity
    {
        public string VehiclePartName { get; set; }
        public string Description { get; set; }
        public MaintenanceTimeFrameType MaintenanceTimeFrameType { get; set; }
        public string InstallationMileage { get; set; }
        public string PartExpiryMileage { get; set; }
        public DateTime PartInstallationDate { get; set; }
        public DateTime PartExpiryDate { get; set; }
        public int VehicleId { get; set; }
        public virtual Vehicle Vehicle { get; set; } 
    }
}