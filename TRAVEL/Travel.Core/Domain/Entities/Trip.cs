
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class Trip : FullAuditedEntity<Guid>
    {
        public string DepartureTime { get; set; }
        public string TripCode { get; set; }
        public bool AvailableOnline { get; set; }
        public string ParentRouteDepartureTime { get; set; }

        public int RouteId { get; set; }
        public virtual Route Route { get; set; }

        public int? VehicleModelId { get; set; }
        public virtual VehicleModel VehicleModel { get; set; }
        public int? ParentRouteId { get; set; }
        public Guid? ParentTripId { get; set; }
    }
}