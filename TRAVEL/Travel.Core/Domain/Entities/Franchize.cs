using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Utils;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Travel.Core.Domain.Entities
{
    public class Franchize : FullAuditedEntity
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public bool IsFirstTimeLogin { get; set; }
        public string OptionalPhoneNumber { get; set; }
        public UserType UserType { get; set; }
        public string Image { get; set; }
        public string RefreshToken { get; set; }
        public string Title { get; set; }
        public string DeviceToken { get; set; }
        public string Referrer {get; set;}
        public string ReferralCode { get; set; }
        public string NextOfKinName { get; set;}
        public string NextOfKinPhone { get; set; }
        public DeviceType LoginDeviceType { get; set; }
        public int? WalletId { get; set; }
        public Gender Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string AccountConfirmationCode { get; set; }
        public string Photo { get; set; }
        public string OTP { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }




    }

 
}