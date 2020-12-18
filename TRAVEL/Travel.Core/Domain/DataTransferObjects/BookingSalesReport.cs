using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class BookingSalesReport
    {
        public decimal Amount { get; set; }
        public int RouteId { get; set; }
        public string Route { get; set; }
        public int DepartureTerminalId { get; set; }
        public string DepartureTerminal { get; set; }
        public string CreatedBy { get; set; }
        public int StateId { get; set; }
        public string Region { get; set; }
        public string State { get; set; }
    }
}
