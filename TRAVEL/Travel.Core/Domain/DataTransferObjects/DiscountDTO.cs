using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class DiscountDTO
    {
        public Guid Id { get; set; }
        public BookingTypes BookingType { get; set; }
        public string BookingTypeName { get; set; }
        public decimal AdultDiscount { get; set; }
        public decimal MinorDiscount { get; set; }
        public decimal MemberDiscount { get; set; }
        public decimal ReturnDiscount { get; set; }
        public decimal AppDiscountIos { get; set; }
        public decimal AppDiscountAndroid { get; set; }
        //this is added for new customer
        public decimal CustomerDiscount { get; set; }
        public decimal AppDiscountWeb { get; set; }
        public decimal AppReturnDiscountIos { get; set; }
        public decimal AppReturnDiscountAndroid { get; set; }
        public decimal AppReturnDiscountWeb { get; set; }
        public decimal PromoDiscount { get; set; }
        public bool Active { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public bool IsDeleted { get; set; }
        

    }
}