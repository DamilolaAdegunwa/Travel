
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class PickupPoint: FullAuditedEntity
    {
        public string Name { get; set; }
        public string PickupTime { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public string Image { get; set; }

        public Guid TripId { get; set; }
        public virtual Trip Trip { get; set; }

        public int? TerminalId { get; set; }
        public virtual Terminal Terminal { get; set; }

        public int? RouteId { get; set; }
        public virtual Route Route { get; set; }
    }
}