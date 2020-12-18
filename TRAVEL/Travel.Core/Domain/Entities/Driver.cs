using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.Entities
{
    public class Driver : FullAuditedEntity
    {
        public string Code { get; set; }
        public string HandoverCode { get; set; }
        public string VehicleRegistrationNumber { get; set; }
        public virtual DriverStatus DriverStatus { get; set; }
        public virtual DriverType DriverType { get; set; }
        public string Name { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Designation { get; set; }
        public DateTime AssignedDate { get; set; }
        public string ResidentialAddress { get; set; }
        public string NextOfKin { get; set; }
        public DateTime? DateOfEmployment { get; set; }
        public string Picture { get; set; }
        public bool Active { get; set; }
        public string NextOfKinNumber { get; set; }
        public string BankName { get; set; }
        public string BankAccount { get; set; }
        public string DeactivationReason { get; set; }
        public string ActivationStatusChangedByEmail { get; set; }

        public int WalletId { get; set; }
        public virtual Wallet Wallet { get; set; }

        public int? MaintenanceWalletId { get; set; }
        public virtual Wallet MaintenanceWallet { get; set; }

        public int NoOfTrips { get; set; }

        public AccountEnrollmentStatus EnrollmentStatus { get; set; }
        public string ConfirmationCode { get; set; }
    }
}