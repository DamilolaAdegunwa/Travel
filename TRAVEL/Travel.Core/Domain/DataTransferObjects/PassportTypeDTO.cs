using System.ComponentModel.DataAnnotations;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class PassportTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int RouteId { get; set; }
        public string RouteName { get; set; }
        public decimal AddOnFare { get; set; }

    }
}