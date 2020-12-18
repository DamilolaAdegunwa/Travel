using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Domain.Entities.Auditing;
using System;

namespace Travel.Core.Domain.Entities
{
    public class Complaint : FullAuditedEntity
    {

        public string FullName { get; set; }
        public string Email { get; set; }

        public ComplaintTypes ComplaintType { get; set; }

        public PriorityLevel PriorityLevel { get; set; }

        public string BookingReference { get; set; }
        public string Message { get; set; }

        public DateTime TransDate { get; set; }
        public bool Responded { get; set; }
        public string RepliedMessage { get; set; }
    }
}
