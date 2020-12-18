
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class VehiclePartPosition: FullAuditedEntity<Guid>
    {
        public string Position { get; set; }
        public VehiclePart VehiclePart { get; set; }
        public int VehiclePartId { get; set; }
    }
}