namespace Travel.Core.Messaging.Sms.Model
{
    public class SMSLiveModel
    {
        public string Message { get; set; }
        public string Sender { get; set; }
        public string[] Recipient { get; set; }
        public string Token { get; set; }
    }
}