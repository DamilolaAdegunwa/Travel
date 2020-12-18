using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class JourneyChartQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? RegionId { get; set; }
        public int? StateId { get; set; }
        public JourneyType JourneyType { get; set; }
        public JourneyStatus JourneyStatus { get; set; }
        public int DepartureTerminalId { get; set; }
        public int DestinationTerminalId { get; set; }

    }
}
