
using Travel.Core.Domain.Entities.Enums;
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class Vehicle : FullAuditedEntity
    {
        public string RegistrationNumber { get; set; }
        public string ChasisNumber { get; set; }
        public string EngineNumber { get; set; }
        public string IMEINumber { get; set; }
        public string Type { get; set; }
        public virtual VehicleStatus Status { get; set; }

        public int VehicleModelId { get; set; }
        public virtual VehicleModel VehicleModel { get; set; }

        public int? LocationId { get; set; }
        public virtual Terminal Location { get; set; }

        public bool IsOperational { get; set; }

        public Driver Driver { get; set; }
        public int? DriverId { get; set; }

        public int? FranchiseId { get; set; }
        public Franchise Franchise { get; set; }

        public int? FranchizeId { get; set; }
        public Franchize Franchize { get; set; }
        public DateTime? PurchaseDate { get; set; }
    }
}