

using Travel.Core.Domain.Entities.Auditing;

namespace Travel.Core.Domain.Entities
{
    public class VehicleModel : FullAuditedEntity
    {
        public string Name { get; set; }
        public int NumberOfSeats { get; set; }
        public string VehicleModelTypeCode { get; set; }
        public int VehicleMakeId { get; set; }
        public virtual VehicleMake VehicleMake { get; set; }
    }
}