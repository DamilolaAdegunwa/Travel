using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class SalesPerBusDTO
    {
        public decimal TotalSales { get; set; }
        public string PhysicalBusRegistrationNumber { get; set; }
        public DateTime DepartureDate { get; set; }
        public int RemainingSeats { get; set; }
        public int NoOfSeats { get; set; }
        public decimal TerminalBookingSales { get; set; }
        public decimal AdvancedBookingSales { get; set; }
        public decimal OnlineBookingSales { get; set; }
        public decimal DriverFee { get; set; }
        public decimal DispatchFee { get; set; }
        public Guid Id { get; set; }

    }
}
