
using Travel.Core.Configuration;

namespace Travel.Core.Messaging.Email
{
    public class SmtpConfig : ISettings
    {
        public bool EnableSSl { get; set; }
        public int Port { get; set; }
        public string Server { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public bool UseDefaultCredentials { get; set; }
    }
}