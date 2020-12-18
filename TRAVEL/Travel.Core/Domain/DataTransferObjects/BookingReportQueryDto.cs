using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class BookingReportQueryDto
    {
        public int? TerminalId { get; set; }
        public int? BookingType { get; set; }

        public DateTime?  StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string Keyword { get; set; }
        public string CreatedBy { get; set; }
        public string ReferenceCode { get; set; }
        public int? BookingStatus { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
