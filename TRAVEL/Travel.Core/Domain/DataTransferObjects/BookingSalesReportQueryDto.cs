using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class BookingSalesReportQueryDto
    {
        public int? RouteId { get; set; }
        public int? TerminalId { get; set; }
        public int? StateId { get; set; }
        public int? PaymentMethod { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
