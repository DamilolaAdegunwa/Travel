using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Travel.Core.Domain.Entities
{
    public class Role : IdentityRole<int>
    {
        public bool IsActive { get; set; }
        public DateTime? CreationTime { get; set; }
        public bool IsDefaultRole { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}