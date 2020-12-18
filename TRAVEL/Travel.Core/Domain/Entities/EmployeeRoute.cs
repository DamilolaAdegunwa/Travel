using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class EmployeeRoute : FullAuditedEntity<long>
    {
        public long EmployeeRouteId { get; set; }
        public bool IsActive { get; set; }

        public int? TerminalId { get; set; }
        public Terminal Terminal { get; set; }
        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int? RouteId { get; set; }
        public Route Route { get; set; }
    }
}