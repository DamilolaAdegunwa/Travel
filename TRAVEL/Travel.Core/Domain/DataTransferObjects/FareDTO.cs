using System.ComponentModel.DataAnnotations;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class FareDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public float? ChildrenDiscountPercentage { get; set; }

        [Required]
        public int RouteId { get; set; }
        public string RouteName { get; set; }

        [Required]
        public int VehicleModelId { get; set; }
        public string VehicleModelName { get; set; }

        public decimal NonIdAmount { get; set; }
    }
}