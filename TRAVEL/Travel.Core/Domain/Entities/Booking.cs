
using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class Booking : FullAuditedEntity
    {
        public string PosReference { get; set; }
        public string BookingReferenceCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public Gender Gender { get; set; }
        public string Email { get; set; }
        public int NoOfTicket { get; set; }
        public string PhoneNumber { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string Address { get; set; }
        public string NextOfKinName { get; set; }
        public string NextOfKinPhoneNumber { get; set; }
        public int NumberOfTicketsPrinted { get; set; }
        public string PickupPointImage { get; set; }
        public DateTime BookingDate { get; set; }
        public int PaymentGateway { get; set; }
        public string SelectedSeats { get; set; }
        public string PayStackReference { get; set; }
        public string PayStackResponse { get; set; }
        public string PayStackWebhookReference { get; set; }
        public string GtbReference { get; set; }
        public string FlutterWaveReference { get; set; }
        public string FlutterWaveResponse { get; set; }
        public string ApprovedBy { get; set; }
        public string GlobalPayReference { get; set; }
        public string GlobalPayResponse { get; set; }

        public string QuickTellerReference { get; set; }
        public string QuickTellerResponse { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public PassengerType PassengerType { get; set; }
        public PickupStatus PickupStatus { get; set; }
        public BookingType BookingType { get; set; }
        public TravelStatus TravelStatus { get; set; }
        [ForeignKey("PayStackReference")]
        public PayStackPaymentResponse PayStackPaymentResponse { get; set; }

        [ForeignKey("PayStackWebhookReference")]
        public PayStackWebhookResponse PayStackWebhookResponse { get; set; }

        public ICollection<VehicleTripRegistration> VehicleTripRegistrations { get; set; }

        public bool IsGhanaRoute { get; set; }
        public string PassportType { get; set; }
        public string PassportId { get; set; }
        public string PlaceOfIssue { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string Nationality { get; set; }

    }
}