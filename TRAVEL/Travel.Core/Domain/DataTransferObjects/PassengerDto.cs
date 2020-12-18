using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class PassengerDto
    {
        public string BookingReferenceCode { get; set; }
        public decimal? Amount { get; set; }
        public string FullName { get; set; }
        public Gender Gender { get; set; }
        public string PhoneNumber { get; set; }
        public int SeatNumber { get; set; }
        public string Route { get; set; }
        public BookingTypes BookingType { get; set; }
        public decimal? Dispatch { get; set; }
    }
}
