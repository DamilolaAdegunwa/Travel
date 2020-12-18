
using Travel.Core.Domain.Entities.Enums;
using System;
using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class Terminal : FullAuditedEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Image { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPersonNo { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public DateTime TerminalStartDate { get; set; }

        public TerminalType TerminalType { get; set; }
        public int StateId { get; set; }
        public virtual State State { get; set; }
        public string TerminalCode { get; set; }
        public bool IsNew { get; set; }
    }
}