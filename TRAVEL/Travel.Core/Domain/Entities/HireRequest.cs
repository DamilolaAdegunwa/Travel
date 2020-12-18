
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class HireRequest : FullAuditedEntity
    {
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
        public DateTime RequestDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public Vehicle Vehicle { get; set; }
        public int? VehicleId { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public string AdditionalRequest { get; set; }
    }
}