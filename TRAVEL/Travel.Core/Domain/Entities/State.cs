using System.Collections.Generic;
using Travel.Core.Domain.Entities.Common;

namespace Travel.Core.Domain.Entities
{
    public class State:Entity
    {
        public string Name { get; set; }

        public  ICollection<Terminal> Terminals { get; set; }

        public int RegionId { get; set; }
        public  Region Region { get; set; }
    }
}
