
using Travel.Core.Domain.Entities.Enums;
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class AccountTransaction : FullAuditedEntity<Guid>
    {
        public AccountTransaction()
        {
        }

        public double Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Narration { get; set; }
        public AccountType AccountType { get; set; }
        public TransactionType TransactionType { get; set; }
        public Guid TransactionSourceId { get; set; }
    }
}