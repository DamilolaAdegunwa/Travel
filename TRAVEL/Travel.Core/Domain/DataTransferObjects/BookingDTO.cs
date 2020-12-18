using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class BookingDTO
    {
        public int Id { get; set; }

        public string BookingReferenceCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public int NoOfTicket { get; set; }
        public string PosReference { get; set; }
        public string TripCode { get; set; }
        public Gender Gender { get; set; }
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string NextOfKinName { get; set; }
        public string NextOfKinPhoneNumber { get; set; }
        public int NumberOfTicketsPrinted { get; set; }
        public string PickupPointImage { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PassengerType PassengerType { get; set; }
        public string SelectedSeats { get; set; }
        public string PayStackReference { get; set; }
        public string PayStackWebhookReference { get; set; }
        public string GtbReference { get; set; }
        public string PayStackResponse { get; set; }
        public string FlutterWaveReference { get; set; }
        public string FlutterWaveResponse { get; set; }
        public string GlobalPayReference { get; set; }
        public string GlobalPayResponse { get; set; }

        public string QuickTellerReference { get; set; }
        public string QuickTellerResponse { get; set; }

        public BookingStatus BookingStatus { get; set; }

        public PickupStatus PickupStatus { get; set; }

        public TravelStatus TravelStatus { get; set; }

        //[ForeignKey("PayStackReference")]
        public PayStackPaymentResponse PayStackPaymentResponse { get; set; }

        //[ForeignKey("GlobalPayReference")]
        //public GlobalPayResponse GlobalPayPaymentResponse { get; set; }

        //[ForeignKey("PayStackWebhookReference")]
        //public PayStackWebhookResponse PayStackWebhookResponse { get; set; }

        //[ForeignKey("GtbReference")]
        //public FoundTransactionResponse FoundTransactionResponse { get; set; }

        //[ForeignKey("FlutterWaveReference")]
        //public FlutterWavePaymentResponse FlutterWavePaymentResponse { get; set; }

        //[ForeignKey("QuickTellerReference")]
        //public QuickTellerPaymentNotification QuickTellerPaymentResponse { get; set; }

        public int PickUpPointId { get; set; }

        public Guid VehicleTripRegistrationId { get; set; }
        //public object PayStackPaymentResponse { get; set; }

        public bool IsGhanaRoute { get; set; }
        public string PassportType { get; set; }
        public string PassportId { get; set; }
        public string PlaceOfIssue { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string Nationality { get; set; }
    }
}