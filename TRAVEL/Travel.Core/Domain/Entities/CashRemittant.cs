
using Travel.Core.Domain.Entities.Enums;
using System;
using Travel.Core.Domain.Entities.Auditing;
namespace Travel.Core.Domain.Entities
{
    public class CashRemittant: AuditedEntity<Guid>
    {
        public string Ticketer { get; set; }
        public string Accountant { get; set; }
        public double Amount { get; set; }
        public int TerminalId { get; set; }
        public virtual Terminal Terminal { get; set; }
        public AccountingStatus AccountingStatus { get; set; }
        public string AuthorisedBy { get; set; }
    }
}