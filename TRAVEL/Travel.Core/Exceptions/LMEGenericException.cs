using System;

namespace Travel.Core.Exceptions
{
    [Serializable]
    public class LMEGenericException : Exception
    {
        public string ErrorCode { get; set; }

        public LMEGenericException(string message) : base(message)
        { }

        public LMEGenericException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}