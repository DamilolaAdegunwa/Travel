using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class DateModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Keyword { get; set; }
        public BookingTypes BookingType { get; set; }
        public int Id { get; set; }
    }

    public class SearchDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Driver { get; set; }
        public string Code { get; set; }
        public string PhysicalBusRegistrationNumber { get; set; }

    }
    //This is for customer reports
}
