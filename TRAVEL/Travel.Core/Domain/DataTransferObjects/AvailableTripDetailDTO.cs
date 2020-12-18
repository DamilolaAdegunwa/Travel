using System;
using System.Collections.Generic;
using System.Linq;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class AvailableTripDetailDTO
    {
        public string RouteName { get; set; }
        public int RouteId { get; set; }
        public Guid TripId { get; set; }
        public string VehicleName { get; set; }
        public string PhysicalBus { get; set; }
        public string DriverCode { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime DateCreated { get; set; }
        public string DepartureTime { get; set; }
        public int AvailableNumberOfSeats { get; set; }
        public decimal FarePrice { get; set; }
        public decimal MemberFare { get; set; }
        public decimal ChildFare { get; set; }
        public decimal AdultFare { get; set; }
        public decimal ReturnFare { get; set; }
        public decimal PromoFare { get; set; }
        public decimal AppFare { get; set; }
        public decimal AppReturnFare { get; set; }
        public string VehicleFacilities { get; set; }
        public bool HasPickup { get; set; }
        public int BookedSeat { get; set; }
        public decimal VehicleModelId { get; set; }
        public string VehicleModel { get; set; }
        public bool IsSub { get; set; }
        public bool IsSubReturn { get; set; }
        public int? RouteIdReturn { get; set; }
        public IEnumerable<int> BookedSeats { get; set; }
        public IEnumerable<int> JetPrimeMoverIncludedSeats => new List<int>() { 14 };


        public int TotalNumberOfSeats { get; set; }
        public IEnumerable<int> AvailableSeats => TotalNumberOfSeats == 13 ? Enumerable.Range(1, TotalNumberOfSeats - 1).Union(JetPrimeMoverIncludedSeats).Except(ExcludedSeats) : Enumerable.Range(1, TotalNumberOfSeats).Except(ExcludedSeats);

        public IEnumerable<int> ExcludedSeats { get; set; }

        public Guid VehicleTripRegistrationId { get; set; }
        public int? ParentRouteId { get; set; }
    }
}