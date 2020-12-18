using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.Entities
{
    public class Expense: FullAuditedEntity<Guid>
    {
        public string Description { get; set; }
        public string Receiver { get; set; }
        public string Issuer { get; set; }
        public double Amount { get; set; }
        public int  TerminalId { get; set; }
        public virtual Terminal Terminal { get; set; }
        public AccountingStatus AccountingStatus { get; set; }
        public string AuthorisedBy { get; set; }
    }
}
