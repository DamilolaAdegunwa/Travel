using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
   public class PickupPointDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Image { get; set; }
        public string Time { get; set; }

        // FK
        public int TerminalId { get; set; }
        public string TerminalName { get; set; }
        public Guid TripId { get; set; }
        public string DepartureTime { get; set; }
        public int RouteId { get; set; }
        public string RouteName { get; set; }

    }
}
