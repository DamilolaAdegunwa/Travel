using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class SalesReportDTO
    {
        public Guid VehicleTripRegistrationId { get; set; }
        public string DriverName { get; set; }
        public string RouteName { get; set; }
        public string PhysicalBusRegistrationNumber { get; set; }
        public int TotalPassengers { get; set; }
        public decimal AmountTotal { get; set; }
        public decimal TotalOnlineSales { get; set; }
        public decimal TotalAdvancedSales { get; set; }
        public decimal TotalTerminalSales { get; set; }
        public decimal TotalSales { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
    }
}
