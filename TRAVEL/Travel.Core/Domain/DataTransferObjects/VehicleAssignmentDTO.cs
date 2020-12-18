namespace Travel.Core.Domain.DataTransferObjects
{
    public class VehicleAssignmentDTO
    {
        public string PryDriver { get; set; }
        public string PryDriverFullName { get; set; }
        public string NewHandOverDriverCode { get; set; }
        public int VehicleId { get; set; }
    }
}
