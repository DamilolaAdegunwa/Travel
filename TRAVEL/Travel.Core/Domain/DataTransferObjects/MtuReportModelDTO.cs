using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{ 
    public class MtuReportModelDTO
    {
        public int Id { get; set; }
        public string VehicleId { get; set; }
        public string RegistrationNumber { get; set; }
        public long CreatorUserId { get; set; } 
        public string DriverCode { get; set; }
        public VehicleStatus2 Status { get; set; }
        public string VehicleStatus { get; set; }
        public DateTime Date { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Notes { get; set; }
        public string Keyword { get; set; }
        public string Picture { get; set; }
        public List<MtuPhoto> MtuPhotos { get; set; }
    }
}
