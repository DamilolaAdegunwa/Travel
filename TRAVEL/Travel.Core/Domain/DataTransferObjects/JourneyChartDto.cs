using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class JourneyChartDto
    {
        public int TotalOutgoingBlownCount { get; set; }
        public int TotalIncomingBlownCount { get; set; }
        public int ExpectedStartupVehicles { get; set; }
        public int TotalOutgoingCount { get; set; }
        public int TotalIncomingCount { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }

    }
}
