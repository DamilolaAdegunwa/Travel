using System.ComponentModel.DataAnnotations;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class MobileUserDTO
    {
        [StringLength(50, MinimumLength = 1, ErrorMessage = "{0} must be between {1} and {2} characters long.")]
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please specify a valid email.")]
        [EmailAddress(ErrorMessage = "Please specify a valid email.")]
        public string Email { get; set; }

        [StringLength(20, MinimumLength = 8, ErrorMessage = "{0} must be between {1} and {2} characters long.")]
        public string PhoneNumber { get; set; }

        public int Gender { get; set; }

        [Required(ErrorMessage = "Password required")]
        [DataType(DataType.Password)]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Minimum of 6 characters required for password")]
        public string Password { get; set; }
        public string ReferralCode { get; set; }
    }
}
