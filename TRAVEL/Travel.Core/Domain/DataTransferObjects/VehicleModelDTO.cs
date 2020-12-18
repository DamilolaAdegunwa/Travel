using System;
using System.ComponentModel.DataAnnotations;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class VehicleModelDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a valid vehicle model name.")]
        public string Name { get; set; }
        public int NumberOfSeats { get; set; }

        public int VehicleMakeId { get; set; }
        public string VehicleMakeName { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}