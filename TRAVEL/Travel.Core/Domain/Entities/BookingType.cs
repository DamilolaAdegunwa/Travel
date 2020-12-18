using Travel.Core.Domain.Entities.Common;

namespace Travel.Core.Domain.Entities
{
    public class BookingType:Entity
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}