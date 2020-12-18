using Travel.Core.Common.Enums;
using System.Collections.Generic;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class GroupedTripsDetailDTO
    {
        public TripType TripType { get; set; }
        public List<AvailableTripDetailDTO> Departures { get; set; }
        public List<AvailableTripDetailDTO> Arrivals { get; set; }
    }
}