
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class Wallet : FullAuditedEntity
    {
        public string WalletNumber { get; set; }
        public decimal Balance { get; set; }
        public string UserType { get; set; }
        public string UserId { get; set; }
        public bool IsReset { get; set; }
        public DateTime? LastResetDate { get; set; }
    }
}