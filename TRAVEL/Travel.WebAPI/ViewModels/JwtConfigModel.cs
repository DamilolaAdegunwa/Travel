using Travel.Core.Configuration;
using System;

namespace Travel.WebAPI.Models
{
    [Serializable]
    public class JwtConfig:ISettings
    {
        public string SecurityKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        /// <summary>
        /// Seconds 
        /// </summary>
        public int TokenDurationInSeconds { get; set; }
    }
}