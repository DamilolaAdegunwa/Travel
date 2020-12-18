using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class BookedTripsQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string PhysicalBusRegisterationNumber { get; set; }
        public int? BookingType { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TerminalId { get; set; }
        public string TerminalName { get; set; }
        public int DepartureTerminal { get; set; }
        public int DestinationTerminal { get; set; }

    }
}
