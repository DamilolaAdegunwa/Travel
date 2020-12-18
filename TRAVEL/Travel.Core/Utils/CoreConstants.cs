namespace Travel.Core.Utils
{
    public abstract class CoreConstants
    {
        public const string DefaultAccount = "administrator@Travel.com";
        public const string WebBookingAccount = "web@libmot.com";
        public const string IosBookingAccount = "ios@libmot.com";
        public const string AndroidBookingAccount = "android@libmot.com";
        public const string DateFormat = "dd MMMM, yyyy";
        public const string TimeFormat = "hh:mm tt";
        public const string SystemDateFormat = "dd/MM/yyyy";

        public class Roles
        {
            public const string Admin = "Administrator";
            //Todo: remove once role creation is available
            public const string T = "Ticketer";
            public const string OM = "Operations Manager";
            public const string BM = "Booking Manager";
            public const string A = "Auditor";
            public const string CC = "Customer Care";
            public const string AC = "Accountant";
            public const string TM = "Terminal Manager";
        }

        public class Url
        {
            public const string PasswordResetEmail = "messaging/emailtemplates/password-resetcode-email.html";
            public const string AccountActivationEmail = "messaging/emailtemplates/account-email.html";
            public const string BookingSuccessEmail = "messaging/emailtemplates/confirm-email.html";
            public const string BookingAndReturnSuccessEmail = "messaging/emailtemplates/confirm-return-email.html";
            public const string ActivationCodeEmail = "messaging/emailtemplates/activation-code-email.html";
            public const string BookingUnSuccessEmail = "messaging/emailtemplates/failed-email.html";
            public const string RescheduleSuccessEmail = "messaging/emailtemplates/reschedule-success.html";
            public const string AdminHireBookingEmail = "messaging/emailtemplates/hirebooking-admin.html";
            public const string CustomerHireBookingEmail = "messaging/emailtemplates/hirebooking.html";
        }
    }
}