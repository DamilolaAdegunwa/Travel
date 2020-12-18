

using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class IdentificationType: FullAuditedEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}