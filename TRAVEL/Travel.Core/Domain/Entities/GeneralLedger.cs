using Travel.Core.Domain.Entities.Auditing;
using System;

namespace Travel.Core.Domain.Entities
{
    public class GeneralLedger:FullAuditedEntity
    {
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public int TransactionTypeId { get; set; }
        public int TransactionSourceId { get; set; }
    }
}