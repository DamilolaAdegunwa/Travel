using Travel.Core.Domain.Entities.Enums;
using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class ComplaintDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public ComplaintTypes ComplaintType { get; set; }
        public string Complaints { get; set; }
        public PriorityLevel PriorityLevel { get; set; }
        public string Priority { get; set; }

        public string BookingReference { get; set; }
        public string Message { get; set; }
        public DateTime CreationTime { get; set; }
        public bool Responded { get; set; }
        public string RepliedMessage { get; set; }
    }
}