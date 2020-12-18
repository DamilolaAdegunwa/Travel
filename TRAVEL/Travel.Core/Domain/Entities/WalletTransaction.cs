
using Travel.Core.Domain.Entities.Enums;
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class WalletTransaction:FullAuditedEntity<Guid>
    {
        public TransactionType TransactionType { get; set; }
        public Guid TransactionSourceId { get; set; }
        public string UserId { get; set; }
        public decimal TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal LineBalance { get; set; }
        public int WalletId { get; set; }
        public virtual Wallet Wallet { get; set; }
    }
}