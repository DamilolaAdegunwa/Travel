using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class HiredTripRequestDTO
    {
        public string Keyword { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
