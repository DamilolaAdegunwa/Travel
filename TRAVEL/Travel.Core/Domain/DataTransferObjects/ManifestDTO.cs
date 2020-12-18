using System;
using System.Collections.Generic;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class ManifestDetailDTO
    {
        public Guid Id { get; set; }
        public bool IsPrinted { get; set; }
        public string PrintedBy { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Dispatch { get; set; }
        public string DispatchSource { get; set; }
        public Guid VehicleTripRegistrationId { get; set; }
        public string VehicleModel { get; set; }
        //public int? VehicleModelId { get; set; }
        //public string Employee { get; set; }
        //public string Refcode { get; set; }
        //public int? RefcodeSeatNumber { get; set; }
        //public int? RefcodeSubrouteId { get; set; }
        //public decimal? RefcodeAmount { get; set; }
        //public long? SeatManagementId { get; set; }
        //public DateTime? ManifestPrintedTime { get; set; }
        //public int? RouteId { get; set; }
        //public string RouteName { get; set; }
       
        public string DriverCode { get; set; }
        public string DriverPhone { get; set; }
        public string DriverName { get; set; }
        public string Route { get; set; }
        public string BusRegistrationNumber { get; set; }
        public IEnumerable<SeatManagementDTO> Passengers { get; set; }
        public int NumberOfSeats { get; set; }
        public int TotalSeats { get; set; }
        public DateTime DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public IList<int> RemainingSeat { get; set; }
        public IList<int> ClashingSeats { get; set; }
        public IList<int> BookSeat { get; set; }
        public IList<int> BookingTypes { get; set; }
        public IList<bool> TicketPrintStatus { get; set; }
        public int RescheduleFee { get; set; }
        public decimal? RerouteFee { get; set; }
        public decimal? TotalSold { get; set; }
        public int RemainingSeatCount { get; set; }
    }

    public class ManifestDTO
    {
        public Guid Id { get; set; }
        public int NumberOfSeats { get; set; }
        public bool IsPrinted { get; set; }
        public bool ManifestPrinted { get; set; }
        public long? SeatManagementId { get; set; }
        public DateTime? ManifestPrintedTime { get; set; }
        public int? RouteId { get; set; }
        public string RouteName { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Dispatch { get; set; }
        public string DispatchSource { get; set; }
        public Guid VehicleTripRegistrationId { get; set; }
        public int? VehicleModelId { get; set; }
        public string VehicleModel { get; set; }

        public string Employee { get; set; }

        public string Refcode { get; set; }
        public int? RefcodeSeatNumber { get; set; }

        public int? RefcodeSubrouteId { get; set; }
        public decimal? RefcodeAmount { get; set; }
    }
    public class ManifestExt
    {

        public Guid ManifestManagementId { get; set; }
        public string DispatchSource { get; set; }
        public int NumberOfSeats { get; set; }
        public string BusRegNum { get; set; }
        public int? RouteId { get; set; }
        public long? Id { get; set; }
        public DateTime? ManifestPrintedTime { get; set; }
        public long? MainBookerId { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Dispatch { get; set; }
        public bool IsPrinted { get; set; }

        // FK
        public Guid VehicleTripRegistrationId { get; set; }

        public string Employee { get; set; }
        public string Otp { get; set; }
        public string Refcode { get; set; }
        public int? RefcodeSeatNumber { get; set; }
        public int? RefcodeSubrouteId { get; set; }
        public decimal? RefcodeAmount { get; set; }
    }
}