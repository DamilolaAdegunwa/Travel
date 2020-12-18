using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    [Serializable]
    public class AccountActivationDTO
    {
        public string ActivationCode { get; set; }
        public string Email { get; set; }
    }
}