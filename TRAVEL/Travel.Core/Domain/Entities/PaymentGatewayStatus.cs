using Travel.Core.Domain.Entities.Common;

namespace Travel.Core.Domain.Entities
{
    public class PaymentGatewayStatus:Entity
    {
        public string Gateway { get; set; }
        public bool Status { get; set; }
    }
}
