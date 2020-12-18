
using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Enums;

namespace Travel.Core.Domain.Entities
{
    public class Route : FullAuditedEntity
    {
        public string Name { get; set; }
        public RouteType Type { get; set; }
        public decimal DispatchFee { get; set; }
        public decimal DriverFee { get; set; }
        public decimal LoaderFee { get; set; }
        public bool AvailableAtTerminal { get; set; }
        public bool AvailableOnline { get; set; }
        public int? ParentRouteId { get; set; }
        public string ParentRoute { get; set; }

        public int DepartureTerminalId { get; set; }
        public  Terminal DepartureTerminal { get; set; }
        public int DestinationTerminalId { get; set; }
        public  Terminal DestinationTerminal { get; set; }
    }
}