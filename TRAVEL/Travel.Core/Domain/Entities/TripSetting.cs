
using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class TripSetting : FullAuditedEntity<Guid>
    {
        public Guid TripSettingId { get; set; }
        public int RouteId { get; set; }
        public WeekDays WeekDays { get; set; }


        public virtual ICollection<TripAvailability> AvailableTrips { get; set; }

    }
}
