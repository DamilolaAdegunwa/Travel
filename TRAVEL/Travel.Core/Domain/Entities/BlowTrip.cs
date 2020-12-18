
using System;
using System.Collections.Generic;
using System.Text;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class BlowTrip : FullAuditedEntity<Guid>
    {
        //public Guid Id { get; set; }
        public string DepartureTerminal { get; set; }
        public string DestinationTerminal { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public int? LocationId { get; set; }
        //public Terminal Location { get; set; }
        //public Driver Driver { get; set; }
        public int? DriverId { get; set; }
        //public Vehicle Vehicle { get; set; }
        public int? VehicleId { get; set; }
        public decimal? Amount { get; set; }
    }
}
