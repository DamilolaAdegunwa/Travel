

using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class VehicleMake : FullAuditedEntity
    {
        public string Name { get; set; }
    }
}