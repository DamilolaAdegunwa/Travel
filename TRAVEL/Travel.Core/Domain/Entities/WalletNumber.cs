

using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class WalletNumber : FullAuditedEntity
    {
        public string WalletPan { get; set; }
        public bool IsActive { get; set; }
    }
}