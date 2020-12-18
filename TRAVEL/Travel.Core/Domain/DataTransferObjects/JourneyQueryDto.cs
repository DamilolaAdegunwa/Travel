using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class JourneyQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? TerminalId { get; set; }

    }
}
