using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Travel.Core.Common.Enums;
using Travel.Core.Domain.Entities;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class BookingDetailDTO
    {
        public BookingDetailDTO()
        {
            Beneficiaries = new List<BeneficiaryDetailDTO>();
        }

        public TripType TripType { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public string PosReference { get; set; }

        public BookingTypes BookingType { get; set; }

        public PassengerType PassengerType { get; set; }

        [Required, Display(Name = "First Name")]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public int? RouteId { get; set; }
        public bool IsSub { get; set; }
        public bool IsSubReturn { get; set; }


        public int? RouteIdReturn { get; set; }
        public string LuggageType { get; set; }


        public int? SubrouteId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? PartCash { get; set; }
        public string PosRef { get; set; }
        public decimal? Discount { get; set; }
        public string BookingReference { get; set; }
        public string TicketNumber { get; set; }
        public string ApprovedBy { get; set; }

        [Required, EmailAddress, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [ DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [Required, Display(Name = "Next-of-Kin Name")]
        public string NextOfKinName { get; set; }

        [Required, Display(Name = "Next-of-Kin Phone")]
        public string NextOfKinPhone { get; set; }

        public string SeatRegistrations { get; set; }

        [Display(Name = "Beneficiaries"), UIHint("BeneficiaryDetail")]
        public List<BeneficiaryDetailDTO> Beneficiaries { get; set; }

        public BookingStatus BookingStatus { get; set; }

        public int? PickUpId { get; set; }
        public PickupStatus PickupStatus { get; set; }
        public int? ReturnPickUpId { get; set; }
        public PickupStatus ReturnPickupStatus { get; set; }

        public bool IsLoggedIn { get; set; }


        public TravelStatus TravelStatus { get; set; }
        public string PayStackReference { get; set; }
        public string PayStackResponse { get; set; }
        //[ForeignKey("PayStackReference")]
        public PayStackPaymentResponse PayStackPaymentResponse { get; set; }


        public string GlobalPayReference { get; set; }
        public string GlobalPayResponse { get; set; }
        //[ForeignKey("GlobalPayReference")]
        //public GlobalPayResponse GlobalPayPaymentResponse { get; set; }

        public string QuickTellerReference { get; set; }
        public string QuickTellerResponse { get; set; }
        //[ForeignKey("QuickTellerReference")]
        //public QuickTellerPaymentNotification QuickTellerPaymentResponse { get; set; }

        public bool HasCoupon { get; set; }
        public string CouponCode { get; set; }

        public bool IsGhanaRoute { get; set; }
        public string PassportType { get; set; }
        public string PassportId { get; set; }
        public string PlaceOfIssue { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string Nationality { get; set; }
        public string PassportTypeId { get; set; }
    }
}
