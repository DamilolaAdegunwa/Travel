using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class DriverDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string HandoverCode { get; set; }
        public virtual DriverStatus DriverStatus { get; set; }
        public virtual DriverType DriverType { get; set; }
        public int NoOfTrips { get; set; }
        public string Name { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Designation { get; set; }
        public DateTime AssignedDate { get; set; }
        public string ResidentialAddress { get; set; }
        public string NextOfKin { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateOfEmployment { get; set; }
        public string Picture { get; set; }
        public bool Active { get; set; }
        public string NextOfKinNumber { get; set; }
        public string BankName { get; set; }
        public string BankAccount { get; set; }
        public string DeactivationReason { get; set; }
        public string ActivationStatusChangedByEmail { get; set; }
        public int WalletId { get; set; }
        public int? MaintenanceWalletId { get; set; }
        public string WalletNumber { get; set; }
        public decimal WalletBalance { get; set; }
        public string Details { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string DriverDetails { get; set; }
    }
}