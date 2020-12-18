namespace Travel.Core.Domain.DataTransferObjects
{
    public class StateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int RegionId { get; set; }
        public string RegionName { get; set; }
    }
}