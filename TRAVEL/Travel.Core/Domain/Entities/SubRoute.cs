

using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class SubRoute : FullAuditedEntity
    {
        public string Name { get; set; }
        public int NameId { get; set; }

        public int RouteId { get; set; }
        public virtual Route Route { get; set; }
    }
}