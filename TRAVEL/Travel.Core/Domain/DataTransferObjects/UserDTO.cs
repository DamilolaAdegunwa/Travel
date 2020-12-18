using Travel.Core.Domain.Entities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class UserDTO
    {
        public long UserId { get; set; }

        public bool IsActive { get; set; }

        public UserType UserType { get; set; }

        [StringLength(50, MinimumLength = 1, ErrorMessage = "{0} must be between {1} and {2} characters long.")]
        [Display(Name = "First Name")]
        [Required]
        public string FirstName { get; set; }

        [StringLength(50, MinimumLength = 1, ErrorMessage = "{0} must be between {1} and {2} characters long.")]
        [Display(Name = "Last Name")]
        [Required]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please specify a valid email.")]
        [EmailAddress(ErrorMessage = "Please specify a valid email.")]
        public string Email { get; set; }

        [StringLength(20, MinimumLength = 8, ErrorMessage = "{0} must be between {1} and {2} characters long.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        public string Image { get; set; }
        public List<string> Roles { get; set; }
        public bool AccountIsDeleted { get; set; }
        public Gender Gender { get; set; }
        public string ReferralCode { get; set; }
    }

    public class UserProfileDTO
    {
        public string NextOfKin { get; set; }

        public string NextOfKinPhone { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string ReferralCode { get; set; }
        public string Address { get; set; }
        public string MiddleName { get; set; }
        public string DateJoined { get; set; }
        public string DateOfBirth { get; set; }
        public string Title { get; set; }
    }
}