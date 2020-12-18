using IdentityModel;
using System.Security.Claims;
using System.Security.Principal;

namespace Travel.WebAPI.Utils
{
    public class UserClaims : ClaimsPrincipal
    {
        public UserClaims(IPrincipal principal) : base(principal)
        {
        }

        public int Id
        {
            get
            {
                var idClaim = FindFirst(JwtClaimTypes.Id);
                if (idClaim == null)
                    return 0;

                return int.Parse(idClaim.Value);
            }
        }

        public string Email
        {
            get
            {
                var emailClaim = FindFirst(JwtClaimTypes.Email);
                if (emailClaim == null)
                    return string.Empty;

                return emailClaim.Value;
            }
        }

        public string UserName
        {
            get
            {
                var usernameClaim = FindFirst(JwtClaimTypes.Name);
                if (usernameClaim == null)
                    return "Anonymous";

                return usernameClaim.Value;
            }
        }

        public string FirstName
        {
            get
            {
                var usernameClaim = FindFirst(JwtClaimTypes.GivenName);

                if (usernameClaim == null)
                    return string.Empty;

                return usernameClaim.Value;
            }
        }

        public string LastName
        {
            get
            {
                var usernameClaim = FindFirst(JwtClaimTypes.FamilyName);

                if (usernameClaim == null)
                    return string.Empty;

                return usernameClaim.Value;
            }
        }
        public string Terminal
        {
            get
            {
                var usernameClaim = FindFirst("location");

                if (usernameClaim == null)
                    return string.Empty;

                return usernameClaim.Value;
            }
        }
    }
}