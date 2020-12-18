using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class SalesSummaryDTO
    {
        public decimal? TodaysSales { get; set; }
        public decimal? LastSales { get; set; }
        public int? TodaysBookings { get; set; }
    }
}
