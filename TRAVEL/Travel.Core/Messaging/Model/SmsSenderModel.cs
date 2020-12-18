using Travel.Core.Messaging.Sms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Travel.Core.Messaging.Model
{
    public class SMSSenderModel : SMSSender
    {
        public string ApiUrl { get; set; }
        public string Method { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public IWebClient WebClient => SMSService.WebClientSource;


        public override Task SendSmsAsync() => Task.Factory.StartNew(() => {
            WebClient.DoRequest(ApiUrl, Method, Body, Headers);
        });

        public override void SendSms() => WebClient.DoRequest(ApiUrl, Method, Body, Headers);
    }
}