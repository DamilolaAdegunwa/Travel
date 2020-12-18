using Travel.Core.Domain.Entities.Auditing;
using System;

namespace Travel.Core.Domain.Entities
{
    public class Franchise : FullAuditedEntity
    {
        //public int Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid Code { get; set; }
        public string PhoneNumber { get; set; }

    }
}