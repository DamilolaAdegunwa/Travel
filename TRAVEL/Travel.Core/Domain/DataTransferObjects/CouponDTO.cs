using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class CouponDTO
    {
        public Guid Id { get; set; }

        public string CouponCode { get; set; }
        public decimal CouponValue { get; set; }
        public CouponType CouponType { get; set; }
        public DurationType DurationType { get; set; }
        public int Duration { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Validity { get; set; }
        public string ValidityStatus { get; set; }
    }
}
