using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class NewTripIdDTO
    {
        public Guid vehicleTripReg { get; set; }

        public Guid newTripId { get; set; }

        public int RouteId { get; set; }
    }
}
