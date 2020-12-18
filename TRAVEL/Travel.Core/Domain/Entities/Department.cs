

using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class Department : FullAuditedEntity
    {
        public string Name { get; set; }
    }
}
