
using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Enums;

namespace Travel.Core.Domain.Entities
{
    public class Referral : FullAuditedEntity<long>
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set;}
        public UserType UserType { get; set; }
        public string  ReferralCode { get; set; }
    }
}