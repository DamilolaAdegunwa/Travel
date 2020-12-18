using Travel.Core.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using Travel.Core.Domain.Extensions;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class VehicleTripRouteSearchDTO
    {
        public TripType TripType { get; set; }

        [Required, Display(Name = "Departure Terminal")]
        public int DepartureTerminalId { get; set; }

        [Required, Display(Name = "Destination Terminal")]
        public int DestinationTerminalId { get; set; }

        [Required, Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; }

        [RequiredIf(nameof(TripType), TripType.Return), Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        public int NumberOfAdults { get; set; }
        public int NumberOfChildren { get; set; }

    }
}
