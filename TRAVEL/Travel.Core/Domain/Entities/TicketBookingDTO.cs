using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.Entities
{
    public class TicketBookingDTO
    {
        public Guid? VehicleTripRegistrationId { get; set; }
        public Guid? TripId { get; set; }
        public long SeatManagementId { get; set; }
        public string Refcode { get; set; }
        public string MainBookerRefcode { get; set; }
        public string RescheduleReferenceCode { get; set; }
        public string RerouteReferenceCode { get; set; }
        public string VehicleModel { get; set; }
        public int SeatNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TicketValidity { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string DepartureTime { get; set; }

        public string NokNumber { get; set; }
        public string NokName { get; set; }
        public DateTime BookedDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        public int? NoofTicket { get; set; }
        public decimal? Amount { get; set; }
        public decimal? RerouteFeeDiff { get; set; }
        public int? HasTravelled { get; set; }

        public BookingStatus? BookingStatus { get; set; }
        public TravelStatus? TravelStatus { get; set; }
        public RescheduleStatus RescheduleStatus { get; set; }
        public RerouteStatus RerouteStatus { get; set; }
        public BookingTypes? BookingType { get; set; }
        public bool IsRescheduled { get; set; }
        public bool IsRerouted { get; set; }
        public int? RouteId { get; set; }
        public int? TerminalId { get; set; }
        public string TerminalName { get; set; }
        public string Email { get; set; }
        public string Route { get; set; }
        public string CreatedBy { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public DateTime? TravalledDate { get; set; }
    }
}
