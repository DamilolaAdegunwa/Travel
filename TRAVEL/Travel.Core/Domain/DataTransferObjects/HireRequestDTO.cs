using System;
using System.ComponentModel.DataAnnotations;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class HireRequestDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string NextOfKinName { get; set; }
        public string NextOfKinPhoneNumber { get; set; }
        public int NumberOfBuses { get; set; }
        public int? VehcleId { get; set; }
        public string AdditionalRequest { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public int TotalCount { get; set; }

    }
}