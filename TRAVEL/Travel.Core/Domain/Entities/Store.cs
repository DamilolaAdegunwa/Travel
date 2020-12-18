
using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Enums;

namespace Travel.Core.Domain.Entities
{
    public class Store : FullAuditedEntity
    {
        public StoreType Type { get; set; }
        public string Name { get; set; }

        public string StoreKeeper { get; set; }
        public int TerminalId { get; set; }
        public virtual Terminal Terminal { get; set; }
    }
}