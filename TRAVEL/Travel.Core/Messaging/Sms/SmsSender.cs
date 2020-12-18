using System.Threading.Tasks;

namespace Travel.Core.Messaging.Sms
{
    public abstract class SMSSender
    {
        public abstract Task SendSmsAsync();
        public abstract void SendSms();
    }
}