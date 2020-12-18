using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.Entities
{
    public class Coupon : AuditedEntity<Guid>
    {
        public string CouponCode { get; set; }
        public decimal CouponValue { get; set; }
        public CouponType CouponType { get; set; }

        public bool Validity { get; set; }
        public DurationType DurationType { get; set; }
        public int Duration { get; set; }
    }
}