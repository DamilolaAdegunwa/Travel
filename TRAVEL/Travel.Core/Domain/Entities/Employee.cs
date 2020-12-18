using Travel.Core.Domain.Entities.Auditing;
using System;

namespace Travel.Core.Domain.Entities
{
    public class Employee : FullAuditedEntity
    {
        public string Otp { get; set; }
        public bool OtpIsUsed { get; set; }
        public string TicketRemovalOtp { get; set; }
        public bool TicketRemovalOtpIsUsed { get; set; }
        public DateTime? OTPLastUsedDate { get; set; }
        public int? OtpNoOfTimeUsed { get; set; }

        public string EmployeeCode { get; set; }
        public DateTime? DateOfEmployment { get; set; }

        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        public int? TerminalId { get; set; }
        public Terminal Terminal { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}