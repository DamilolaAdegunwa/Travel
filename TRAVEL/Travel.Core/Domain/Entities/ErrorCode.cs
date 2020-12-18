using System;
using System.Collections.Generic;
using System.Text;
using Travel.Core.Domain.Entities.Common;

namespace Travel.Core.Domain.Entities
{
   public class ErrorCode:Entity
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }
}
