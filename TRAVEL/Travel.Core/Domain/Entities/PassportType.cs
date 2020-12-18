using Travel.Core.Domain.Entities.Common;

namespace Travel.Core.Domain.Entities
{
    public class PassportType:Entity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int RouteId { get; set; }
        public decimal AddOnFare { get; set; }
    }
}