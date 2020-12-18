

using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class VehiclePart : FullAuditedEntity
    {
        public string Name { get; set; }
        public int CheckThreshold { get; set; }
        public int RefillThreshold { get; set; }
        public int HubCheckThreshold { get; set; }
        public int HubRefillThreshold { get; set; }
        public int CentralCheckThreshold { get; set; }
        public int CentralRefillThreshold { get; set; }

        public int VehicleModelId { get; set; }
        public virtual VehicleModel VehicleModel { get; set; }
    }
}