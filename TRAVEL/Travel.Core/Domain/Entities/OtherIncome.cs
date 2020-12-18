
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class OtherIncome: AuditedEntity<Guid>
    {
        public string PaymentName { get; set; }
        public string PaymentDescription { get; set; }
        public string Issuer { get; set; }
        public double Amount { get; set; }
        public int TerminalId { get; set; }
        public  Terminal Terminal { get; set; }
        public string TerminalName { get; set; }
    }
}