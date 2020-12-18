
using Travel.Core.Domain.Entities.Enums;
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class Discount : AuditedEntity<Guid>
    {
        public BookingTypes BookingType { get; set; }
        public decimal AdultDiscount { get; set; }
        public decimal MinorDiscount { get; set; }
        public decimal MemberDiscount { get; set; }
        public decimal ReturnDiscount { get; set; }
        public decimal AppDiscountIos { get; set; }
        public decimal AppDiscountAndroid { get; set; }
        public decimal AppDiscountWeb { get; set; }
        public decimal AppReturnDiscountIos { get; set; }
        public decimal AppReturnDiscountAndroid { get; set; }
        public decimal AppReturnDiscountWeb { get; set; }
        public decimal PromoDiscount { get; set; }
        public bool Active { get; set; }
        public decimal CustomerDiscount { get; set; }
    }
}