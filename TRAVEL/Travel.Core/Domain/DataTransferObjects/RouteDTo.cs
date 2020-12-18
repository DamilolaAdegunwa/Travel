using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class RouteDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RouteType RouteType { get; set; }
        public decimal DispatchFee { get; set; }
        public decimal DriverFee { get; set; }
        public decimal LoaderFee { get; set; }
        public int? ParentRouteId { get; set; }
        public string ParentRouteName { get; set; }
        public bool Inworkshop { get; set; }
        public JourneyType JourneyType { get; set; }
        public int DepartureTerminalId { get; set; }
        public string DepartureTerminalName { get; set; }
        public bool AvailableAtTerminal { get; set; }
        public bool AvailableOnline { get; set; }
        public int DestinationTerminalId { get; set; }
        public string DestinationTerminalName { get; set; }

        public int RouteTypeId { get; set; }
        public string RouteTypeName { get; set; }

        
    }
}