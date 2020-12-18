using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.Entities
{
    public class JourneyManagement : FullAuditedEntity<Guid>
    {
        public DateTime? ActualTripStartTime { get; set; }

        public DateTime? TripStartTime { get; set; }
        public DateTime? TripEndTime { get; set; }
        public Guid? TransloadedJourneyId { get; set; }
        public DateTime JourneyDate { get; set; }
        public string ApprovedBy { get; set; }
        public string ReceivedBy { get; set; }
        public decimal DispatchFee { get; set; }
        public decimal CaptainFee { get; set; }
        public decimal LoaderFee { get; set; }

        public Guid VehicleTripRegistrationId { get; set; }
        public virtual VehicleTripRegistration VehicleTripRegistration { get; set; }
        public JourneyStatus JourneyStatus { get; set; }
        public string DenialReason { get; set; }
        public JourneyType JourneyType { get; set; }
        public int CaptainTripStatus { get; set; }

    }
}
