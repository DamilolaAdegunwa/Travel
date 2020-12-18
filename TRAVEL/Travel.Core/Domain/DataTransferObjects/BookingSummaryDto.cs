using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class BookingSummaryDto
    {
        public int? OnlineChannelCount { get; set; }
        public int? AdvancedBookingCount { get; set; }
        public int? TerminalBookingCount { get; set; }
    }
}
