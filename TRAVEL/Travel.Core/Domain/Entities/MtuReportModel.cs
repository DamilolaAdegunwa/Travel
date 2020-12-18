using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.Entities
{
    public class MtuReportModel : FullAuditedEntity
    {
        public string Email { get; set; }
        public string Notes { get; set; }
        public string FullName { get; set; }
        public string VehicleId { get; set; }
        public string RegistrationNumber { get; set; }
        public string DriverId { get; set; }
        public VehicleStatus2 Status { get; set; }
        public string Picture { get; set; }
    }
}
