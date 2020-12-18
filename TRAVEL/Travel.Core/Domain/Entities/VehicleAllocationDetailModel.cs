using Travel.Core.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Text;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class VehicleAllocationDetailModel : FullAuditedEntity
    {
        public int? DriverId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public int? DestinationTerminal { get; set; }
        public string UserEmail { get; set; }
    }
}
