using Travel.Core.Domain.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class BeneficiaryDetailDTO
    {
        [Required, Display(Name = "Beneficiary Name")]
        public string FullName { get; set; }
        public int SeatNumber { get; set; }

        [Required, Display(Name = "Gender")]
        public Gender Gender { get; set; }
        public PassengerType PassengerType { get; set; }
    }
}
