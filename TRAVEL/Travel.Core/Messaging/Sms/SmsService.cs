using Travel.Core.Messaging.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Travel.Core.Collections.Extensions;
using Travel.Core.Messaging.Sms;
using Travel.Core.Messaging.Sms.Model;
using Travel.Core.Timing;

namespace Travel.Core.Messaging.Sms
{
    public class SMSService : ISMSService
    {
        public static IWebClient WebClientSource;

        public SMSService(IWebClient webClient)
        {
            WebClientSource = webClient;
        }

        public void SendSMSNow(string message, string sender = "", params string[] recipient)
        {
            var model = new SMSLiveModel
            {
                Message = message,
                Sender = sender,
                Recipient = recipient
            };

            //Task.WaitAll(KonnectKirusaSMS(model).SendSmsAsync());
            Task.WaitAll(OgoSMS(model).SendSmsAsync());
        }

        public SMSSenderModel OgoSMS(SMSLiveModel model)
        {
            var smsBody = model.Message;
            var recipient = model.Recipient.ArrayToCommaSeparatedString().UrlEncode();
            

            var url = "http://www.ogosms.com/dynamicapi/?username=LibraMotors&password=Libra123$&sender=LIBMOT.COM&numbers=" + recipient + "&message=" + smsBody;
            var senderModel = new SMSSenderModel
            {
                ApiUrl = url,
                Method = "GET"
            };
            return senderModel;
        }

        public SMSSenderModel KonnectKirusaSMS(SMSLiveModel model)
        {
            var url = "https://konnect.kirusa.com/api/v1/Accounts/ESTu+oOQqpnXkJGHxGrlMA==/Messages";
            var senderModel = new SMSSenderModel
            {
                ApiUrl = url,
                Method = "POST", 
                Body = JsonConvert.SerializeObject(new
                {
                    from= "23434565",
                    id = $"{Clock.Now.ToFileTimeUtc()}",
                    sender_mask = "LIBMOT",
                    to = model.Recipient,
                    body = model.Message,
                }),
                Headers = new Dictionary<string, string>()
                {
                    ["Authorization"] = "vuHX4LeAUpCvI2CCmTeAd27I1ZiIW0CdM859hQZqVaY="
                }
            };

            return senderModel;
        }
    }
}