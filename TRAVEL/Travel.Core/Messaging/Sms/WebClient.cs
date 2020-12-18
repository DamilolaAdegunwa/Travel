using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Travel.Core.Messaging.Sms
{
    public class WebClient : IWebClient
    {
        private readonly ILogger _logger;
        static readonly Encoding Encoding = Encoding.UTF8;

        public WebClient(ILogger<WebClient> logger)
        {
            _logger = logger;
        }

        protected virtual WebRequest SetupRequest(string method, string url, Dictionary<string, string> headers, string contentType, X509Certificate clientCertificate)
        {
            var req = WebRequest.Create(url) as HttpWebRequest;
            req.Method = method;
            //req.PreAuthenticate = true;
            req.Timeout = 900000; //20 sec
            if (method == "POST" || method == "PUT")
                req.ContentType = contentType ?? "application/json";
            if (headers != null) {
                foreach (var header in headers) {
                    req.Headers.Add(header.Key, header.Value);
                }
            }

            if (clientCertificate != null) {
                req.ClientCertificates.Add(clientCertificate);
            }

            return req;
        }

        static string GetResponseAsString(WebResponse response)
        {
            if (response != null) {
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding)) {
                    return sr.ReadToEnd();
                }
            }
            return "";
        }

        public string DoRequest(string endpoint, string method = "GET", string body = null, Dictionary<string, string> headers = null, string contentType = null, X509Certificate clientCertificate = null)
        {
            string result = "";
            try {

                WebRequest req = SetupRequest(method, endpoint, headers, contentType, clientCertificate);
                if (body != null) {
                    byte[] bytes = Encoding.GetBytes(body.Trim());
                    req.ContentLength = bytes.Length;
                    using (Stream st = req.GetRequestStream()) {
                        st.Write(bytes, 0, bytes.Length);
                    }
                }

                try {
                    using (WebResponse resp = req.GetResponse()) {
                        result = GetResponseAsString(resp);
                    }
                }
                catch (WebException ex) {
                    if (ex.Response != null) {
                        var asdf = GetResponseAsString(ex.Response);
                    }

                    _logger.LogError(ex.Message);
                }
                return result;
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
            }
            return result;
        }
    }
}