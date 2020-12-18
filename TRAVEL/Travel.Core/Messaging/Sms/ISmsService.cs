namespace Travel.Core.Messaging.Sms
{
    public interface ISMSService
    {
        void SendSMSNow(string message, string sender = "", params string[] recipient);
    }
}