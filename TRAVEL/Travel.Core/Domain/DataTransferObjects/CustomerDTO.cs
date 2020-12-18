using Travel.Core.Domain.Entities.Enums;

namespace Travel.Core.DataTransferObjects
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string CustomerCode { get; set; }
        public Gender Gender { get; set; }
        public string NextOfKinName { get; set; }
        public string NextOfKinPhone { get; set; }

        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        //public DeviceType LoginDeviceType { get; set; }

        //public int? WalletId { get; set; }
        //public string WalletNumber { get; set; }
        //public string DeviceToken { get; set; }
        public string ReferralCode { get; set; }
        public string OptionalPhoneNumber { get; set; }
        public string Password { get; set; }
    }
}