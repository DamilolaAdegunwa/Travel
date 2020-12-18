using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class RemainingSeatDTO
    {
        public Guid VehicleTripRegistrationId { get; set; }
        public List<int> RemainingSeat { get; set; }
    }
}