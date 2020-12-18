using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class EmployeeRouteDTO
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }

        public int? TerminalId { get; set; }
        public string TerminalName { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int? RouteId { get; set; }
        public string RouteName { get; set; }
    }
}
