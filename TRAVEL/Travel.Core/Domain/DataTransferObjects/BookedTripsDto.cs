using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class BookedTripsDto
    {
        public int SeatsBooked { get; set; }
        public int AvailableSeats { get; set; }
        public DateTime DepartureDate { get; set; }
        public Guid VehicleTripRegistrationId { get; set; }
        public string DepartureTime { get; set; }
        public string PhysicalBusRegistrationNumber { get; set; }
        public string RouteName { get; set; }
        public decimal Revenue { get; set; }
        public int DepartureTerminal { get; set; }
        public int DestinationTerminal { get; set; }
        public bool IsPrinted { get; set; }

    }
}
