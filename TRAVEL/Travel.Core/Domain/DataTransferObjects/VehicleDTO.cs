using Travel.Core.Domain.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class VehicleDTO
    {
        public string Details { get; set; }
        public int Id { get; set; }
        public string RegistrationNumber { get; set; }
        public string ChasisNumber { get; set; }
        public string EngineNumber { get; set; }
        public string IMEINumber { get; set; }
        public string Type { get; set; }
        public VehicleStatus VehicleStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public int VehicleModelId { get; set; }
        public string VehicleModelName { get; set; }
        public bool IsOperational { get; set; }
        public string DriverName { get; set; }
        public int? DriverId { get; set; }
        public string DriverCode { get; set; }

        //public int? FranchiseId { get; set; }
        public int? FranchizeId { get; set; }
        public string FranchiseName { get; set; }
        public string DriverNo { get; set; }

    }


    public class VehicleAllocationDTO
    {
        public string RegistrationNumber { get; set; }
        public int? LocationId { get; set; }
        public int Id { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPersonNo { get; set; }
        public int TerminalId { get; set; }      
        public string TerminalName { get; set; }
        public int Type { get; set; }
        public string DriverName { get; set; }
        public string Details { get; set; }
        public int? DriverId { get; set; }
        public int DrvId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public int VehicleId { get; set; }

    }


}