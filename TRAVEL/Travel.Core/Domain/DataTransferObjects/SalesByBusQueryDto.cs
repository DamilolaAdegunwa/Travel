using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class SalesByBusQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
