using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class JourneyDetailDisplayDTO
    {
        public string DriverCode { get; set; }
        public string PhysicalBusRegistrationNumber { get; set; }
        public string Name { get; set; }
        public string DriverName { get; set; }
    }
}
