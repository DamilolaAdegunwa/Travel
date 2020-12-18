using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class EmployeeDTO
    {
        public string Email { get; set; }
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


       
        public string EmployeeCode { get; set; }
        public DateTime? DateOfEmployment { get; set; }

        public string FullName => FirstName + " " + LastName;
        public string MiddleName { get; set; }
        public Gender Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string EmployeePhoto { get; set; }
        public string NextOfKin { get; set; }
        public string NextOfKinPhone { get; set; }

        //public int? WalletId { get; set; }
        //public string WalletNumber { get; set; }

        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        public int? TerminalId { get; set; }
        public string TerminalName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public string Otp { get; set; }
        public bool OtpIsUsed { get; set; }
        public string TicketRemovalOtp { get; set; }
        public bool TicketRemovalOtpIsUsed { get; set; }
        public DateTime? OTPLastUsedDate { get; set; }
        public int? OtpNoOfTimeUsed { get; set; }
    }
}