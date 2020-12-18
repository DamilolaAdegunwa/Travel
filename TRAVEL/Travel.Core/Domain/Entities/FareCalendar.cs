using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.Entities
{
    public class FareCalendar : FullAuditedEntity
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public FareType FareType { get; set; }
        public FareAdjustmentType FareAdjustmentType { get; set; }
        public FareParameterType FareParameterType { get; set; }
        public decimal FareValue { get; set; }

        public int? RouteId { get; set; }
        public virtual Route Route { get; set; }

        public int? TerminalId { get; set; }
        public virtual Terminal Terminal { get; set; }

        public int? VehicleModelId { get; set; }
        public virtual VehicleModel VehicleModel { get; set; }
    }
}