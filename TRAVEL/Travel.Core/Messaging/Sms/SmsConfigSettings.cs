using Travel.Core.Configuration;

namespace Travel.Core.Messaging.Sms
{
    public abstract class SMSConfigSettings : ISettings
    {
        public string Sender { get; set; }
    }
}