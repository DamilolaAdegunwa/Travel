using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class SeatManagementDTO
    {
        public string refCode;
        public long Id { get; set; }
        public Guid? TripId { get; set; }
        public long? BookingId { get; set; }
        public int SeatNumber { get; set; }
        public int RemainingSeat { get; set; }
        public string BookingReferenceCode { get; set; }
        public string RescheduleReferenceCode { get; set; }
        public string TripCode { get; set; }
        public string MainBookerReferenceCode { get; set; }
        public string PhoneNumber { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByTerminal { get; set; }
        public string NextOfKinName { get; set; }
        public string NextOfKinPhoneNumber { get; set; }
        public string FullName { get; set; }
        public string DepartureTime { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public string VehicleName { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime NewDate { get; set; }
        public decimal? Amount { get; set; }
        public decimal? PartCash { get; set; }
        public string POSReference { get; set; }
        public decimal? Discount { get; set; }
        public decimal? RerouteFeeDiff { get; set; }
        public decimal? UpgradeDowngradeDiff { get; set; }
        public bool IsMainBooker { get; set; }
        public bool IsPrinted { get; set; }
        public bool IsCrossSell { get; set; }
        public bool IsSub { get; set; }
        public bool IsSubReturn { get; set; }
        public string OnlineSubRouteName { get; set; }

        public bool HasReturn { get; set; }
        public bool IsReturn { get; set; }
        public bool IsRescheduled { get; set; }
        public bool IsRerouted { get; set; }
        public bool IsUpgradeDowngrade { get; set; }
        public int? NoOfTicket { get; set; }
        public string PickupPointImage { get; set; }
        public bool FromTransload { get; set; }
        public int? RouteId { get; set; }
        public int? PreviousRouteId { get; set; }
        public string RouteName { get; set; }
        public string ReschedulePayStackResponse { get; set; }
        public int? SubRouteId { get; set; }
        public string SubRouteName { get; set; }
        public int? PickUpPointId { get; set; }
        public string PickupPointName { get; set; }

        public bool Rated { get; set; }
        public int? VehicleModelId { get; set; }
        public string LuggageType { get; set; }
        //FK
        public BookingStatus BookingStatus { get; set; }
        public PickupStatus PickupStatus { get; set; }
        public TravelStatus TravelStatus { get; set; }
        public UpgradeType UpgradeType { get; set; }
        public PassengerType PassengerType { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public BookingTypes BookingType { get; set; }
        public RescheduleStatus RescheduleStatus { get; set; }
        public RescheduleMode RescheduleMode { get; set; }
        public RerouteStatus RerouteStatus { get; set; }
        public RerouteMode RerouteMode { get; set; }
        public Gender Gender { get; set; }
        public Guid? VehicleTripRegistrationId { get; set; }
        public string VehicleTripRegistration { get; set; }
        public string DepartureTerminalName { get; set; }
        public string DestinationTerminalName { get; set; }
        public bool IsExpired { get; set; }
        public bool HasCoupon { get; set; }
        public string CouponCode { get; set; }
        public int DepartureTerminald { get; set; }
        public int DestinationTerminalId { get; set; }
        public RescheduleType RescheduleType { get; set; }
        public DateTime? RescheduleDate { get; set; }
        public DateTime? LastModificationTime { get; set; }

        public bool IsGhanaRoute { get; set; }
        public string PassportType { get; set; }
        public string PassportId { get; set; }
        public string PlaceOfIssue { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string Nationality { get; set; }
    }
}