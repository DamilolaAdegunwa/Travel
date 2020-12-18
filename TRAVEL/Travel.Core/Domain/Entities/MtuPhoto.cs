using Travel.Core.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Domain.Entities
{
    public class MtuPhoto: FullAuditedEntity
    {
        public string FileName { get; set; }
        public int MtuReportModelId { get; set; }
    }
}
