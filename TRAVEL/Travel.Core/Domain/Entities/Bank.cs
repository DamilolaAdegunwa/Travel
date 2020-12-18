using Travel.Core.Domain.Entities.Common;

namespace Travel.Core.Domain.Entities
{
    public class Bank : Entity
    {
        public string AccountType { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Name { get; set; }
    }
}