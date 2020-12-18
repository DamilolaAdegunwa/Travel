using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class VehicleMakeDTO
    {
       public int Id { get; set; }
       public string Name { get; set; }
       public DateTime? DateCreated { get; set; }
    }
}