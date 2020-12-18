using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Travel.Core.Messaging.Sms
{
    public interface IWebClient
    {
        string DoRequest(string endpoint, string method = "GET", string body = null, Dictionary<string, string> headers = null, string contentType = null, X509Certificate clientCertificate = null);
    }
}
