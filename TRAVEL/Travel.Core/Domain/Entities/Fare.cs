

using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class Fare : FullAuditedEntity
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public float? ChildrenDiscountPercentage { get; set; }

        public int RouteId { get; set; }
        public virtual Route Route { get; set; }

        public int VehicleModelId { get; set; }
        public virtual VehicleModel VehicleModel { get; set; }
        public decimal NonIdAmount { get; set; }

    }
}