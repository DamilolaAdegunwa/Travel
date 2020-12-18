using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class CustomerCouponRegistration : FullAuditedEntity<long>
    {
        public string CouponCode { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
    }
}