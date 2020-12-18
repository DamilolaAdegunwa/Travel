using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class RescheduleDTO
    {
        public long SeatManagementId { get; set; }
        public int? RouteId { get; set; }
        public int? PreviousRouteId { get; set; }
        public Guid? TripId { get; set; }
        public Guid? VehicleTripRegistrationId { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string RouteName { get; set; }
        public string DepartureTime { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime NewDate { get; set; }
        public RescheduleMode RescheduleMode { get; set; }
        public RerouteMode RerouteMode { get; set; }
        public decimal? Amount { get; set; }
        public decimal? NewRouteAmount { get; set; }
        public decimal? PreviousRouteAmount { get; set; }
        public int SeatNumber { get; set; }
        public string BookingReferenceCode { get; set; }
        public int? VehicleModel { get; set; }
        public RescheduleType RescheduleType { get; set; }
        public DateTime? RescheduleDate { get; set; }
        //new
        public decimal? RerouteFeeDiff { get; set; }
        public decimal? PreviousBookingAmount { get; set; }
        public decimal? NewBookingAmount { get; set; }
        public string vType { get; set; }
        public RescheduleStatus RescheduleStatus { get; set; }

    }
}