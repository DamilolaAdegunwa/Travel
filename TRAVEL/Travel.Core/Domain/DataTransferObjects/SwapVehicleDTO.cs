using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
   public class SwapVehicleDTO
    {
        public int VehicleId { get; set; }
        public Guid VehicleTripRegistrationId { get; set; }
        public string DriverCode { get; set; }
        public string OriginalDriverCode { get; set; }
    }
}