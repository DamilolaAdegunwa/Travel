

using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class Position : FullAuditedEntity
    {
        public string Name { get; set; }

    }
}