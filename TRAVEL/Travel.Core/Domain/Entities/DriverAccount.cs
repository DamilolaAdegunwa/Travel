
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class DriverAccount: FullAuditedEntity<Guid>
    {
        public string DriverCode { get; set; }
        public string Password { get; set; }
        public string ConfirmationCode { get; set; }

        public string DeviceToken { get; set; }
    }
}