
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class TripAvailability: FullAuditedEntity<Guid>
    {
        public string EbmUsername { get; set; }
        public string AssginedVehicle { get; set; }

        public virtual TripSetting TripSetting { get; set; }
        public Guid TripSettingId { get; set; }

        public virtual Trip Trip { get; set; }
        public Guid TripId { get; set; }
    }
}