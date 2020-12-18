using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Core.Configuration
{
    public class AppConfig : ISettings
    {
        public string AppEmail { get; set; }
        public string HiredBookingEmail { get; set; }
        public string OTPMaxperday { get; set; }
        public string MtuSms { get; set; }
    }

    public class BookingConfig : ISettings
    {
        public string CampTripEndDate { get; set; }
        public string TerminalKey { get; set; }
        public string BookingCountReciever { get; set; }
        public string HHEx { get; set; }
        public string HHSEx { get; set; }
        public string HExHSEx { get; set; }
    }

    public class PaymentConfig : ISettings
    {
        public class Paystack
        {
            public string Secret { get; set; }
        }
    }
}
