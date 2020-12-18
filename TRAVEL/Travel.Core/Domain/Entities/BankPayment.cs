
using Travel.Core.Domain.Entities.Enums;
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class BankPayment : FullAuditedEntity
    {
        public DateTime PaymentDate { get; set; }
        public int BankId { get; set; }
        public string TellerNumber { get; set; }
        public string Depositor { get; set; }
        public double Amount { get; set; }
        public int TerminalId { get; set; }
        public virtual Terminal Terminal { get; set; }
        public virtual Bank Bank { get; set; }
        public AccountingStatus AccountingStatus { get; set; }
        public string AuthorisedBy { get; set; }
    }
}