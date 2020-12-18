
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class AccountSummary : FullAuditedEntity<Guid>
    {
        public AccountSummary()
        {
        }

        public string AccountName { get; set; }
        public double Balance { get; set; }
    }
}