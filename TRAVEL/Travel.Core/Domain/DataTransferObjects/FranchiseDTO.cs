using Travel.Core.Domain.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class FranchiseDTO
    {
        //up
        public int Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid Code { get; set; }
        public string PhoneNumber { get; set; }
    }
}