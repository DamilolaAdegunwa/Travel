
using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class WalletDTO
    {
        public int Id { get; set; }
        public string WalletNumber { get; set; }
        public decimal Balance { get; set; }
        public string UserType { get; set; }
        public string UserId { get; set; }
        public string WalletOwnerName { get; set; }
        public bool IsReset { get; set; }
        public DateTime? LastResetDate { get; set; }
        public string PartnerName { get; set; }

    }
}