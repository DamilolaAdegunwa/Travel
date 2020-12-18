using Travel.Core.Domain.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class TerminalDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter a valid terminal name.")]
        public string Name { get; set; }
        public string Code { get; set; }
        public string Image { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPersonNo { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public bool IsNew { get; set; }
        public DateTime? StartDate { get; set; }
        public TerminalType TerminalType { get; set; }
        public BookingTypes BookingType { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int RouteId { get; set; }
        public string TerminalCode { get; set; }

    }

    public class AssignRouteModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Code { get; set; }
        public string Image { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPersonNo { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public bool IsNew { get; set; }
        public DateTime? StartDate { get; set; }
        public TerminalType TerminalType { get; set; }
        public BookingTypes BookingType { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int RouteId { get; set; }
        public long? EmployeeId { get; set; }
    }

}