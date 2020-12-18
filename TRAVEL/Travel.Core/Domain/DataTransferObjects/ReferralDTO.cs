using Travel.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.DataTransferObjects
{
   public  class ReferralDTO
    {
        public long RefferalId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserType UserType { get; set; }
        public string ReferralCode { get; set; }
    }
}
