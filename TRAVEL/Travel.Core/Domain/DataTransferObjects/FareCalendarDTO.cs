using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class FareCalendarDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int? RouteId { get; set; }
        public string RouteName { get; set; }

        public int? TerminalId { get; set; }
        public string TerminalName { get; set; }

        public int? VehicleModelId { get; set; }
        public string VehicleModelName { get; set; }
        public string FareAdjustmentTypeName { get; set; }
        public string FareParameterTypeName { get; set; }
        public string FareTypeName { get; set; }
        public FareType FareType { get; set; }
        public FareAdjustmentType FareAdjustmentType { get; set; }
        public FareParameterType FareParameterType { get; set; }
        public decimal FareValue { get; set; }
    }
}