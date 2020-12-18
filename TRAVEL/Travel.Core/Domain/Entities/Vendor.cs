
using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Enums;

namespace Travel.Core.Domain.Entities
{
    public class Vendor : FullAuditedEntity
    {
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public virtual VendorType VendorType { get; set; }
    }
}