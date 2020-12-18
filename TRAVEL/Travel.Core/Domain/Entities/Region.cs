

using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class Region : FullAuditedEntity
    {
        public string Name { get; set; }
    }
}