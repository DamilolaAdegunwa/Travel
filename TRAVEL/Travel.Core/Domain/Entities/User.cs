
using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Utils;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Travel.Core.Domain.Entities.Auditing;
using Travel.Core.Domain.Entities.Common;

namespace Travel.Core.Domain.Entities
{
    public class User : IdentityUser<int>, IHasCreationTime, IHasDeletionTime, ISoftDelete, IHasModificationTime
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public bool IsFirstTimeLogin { get; set; }
        public string OptionalPhoneNumber { get; set; }
        public UserType UserType { get; set; }
        public string Image { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? DeletionTime { get; set; }
        public bool IsDeleted { get; set; }
        public string RefreshToken { get; set; }

        public string Title { get; set; }
        public string DeviceToken { get; set; }
        public string Referrer { get; set; }
        public string ReferralCode { get; set; }
        public string NextOfKinName { get; set; }
        public string NextOfKinPhone { get; set; }
        public DeviceType LoginDeviceType { get; set; }
        public int? WalletId { get; set; }
        public virtual Wallet Wallet { get; set; }
        public Gender Gender { get; set; }
        public string DateOfBirth { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public string AccountConfirmationCode { get; set; }
        public string Photo { get; set; }
        public string OTP { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }

    public static class UserExtensions
    {
        public static bool IsDefaultAccount(this User user)
        {
            return CoreConstants.DefaultAccount == user.UserName;
        }

        public static bool IsNull(this User user)
        {
            return user == null;
        }

        public static bool IsConfirmed(this User user)
        {
            return user.EmailConfirmed || user.PhoneNumberConfirmed;
        }

        public static bool AccountLocked(this User user)
        {
            return user.LockoutEnabled == true;
        }

        public static bool HasNoPassword(this User user)
        {
            return string.IsNullOrWhiteSpace(user.PasswordHash);
        }
    }
}