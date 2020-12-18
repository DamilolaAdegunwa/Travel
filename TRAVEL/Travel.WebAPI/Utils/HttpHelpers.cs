using System.Net;

namespace Travel.WebAPI.Utils
{
    public static class HttpHelpers
    {
        public static string GetStatusCodeValue(this HttpStatusCode code) {
            return ((int)code).ToString();
        }
    }
}