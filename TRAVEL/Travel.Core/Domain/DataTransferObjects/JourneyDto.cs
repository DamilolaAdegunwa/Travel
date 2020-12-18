using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class JourneyDto
    {
        public string ApprovedBy { get; set; }
        public string ReceivedBy { get; set; }
        public decimal DispatchFee { get; set; }
        public decimal DriverFee { get; set; }
        public decimal LoaderFee { get; set; }
        public JourneyStatus JourneyStatus { get; set; }


        public JourneyType JourneyType { get; set; }


        public bool ManifestPrinted { get; set; }
        public string DepartureTime { get; set; }
        public string TripCode { get; set; }
        public bool AvailableOnline { get; set; }


        public string RouteName { get; set; }
        public string DepartureTerminalName { get; set; }
        public string DestinationTerminalName { get; set; }
        public DateTime DepartureDate { get; set; }
        public string AppovedBy { get; set; }
        public string PhysicalBusRegistrationNumber { get; set; }
        public Guid JourneyManagementId { get; set; }
    }
}
