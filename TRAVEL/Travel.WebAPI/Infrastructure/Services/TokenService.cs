using Travel.Core.DataTransferObjects;
using Travel.Core.Timing;
using Travel.WebAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Travel.WebAPI.Infrastructure.Services
{
    public interface ITokenService
    {
        int TokenMaxMinute { get; }
        TokenDTO GenerateAccessTokenFromClaims(params Claim[] claims);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }

    public class TokenService : ITokenService
    {
        private readonly JwtConfig _jwtConfig;

        public TokenService(IOptions<JwtConfig> jwtConfig)
        {
            _jwtConfig = jwtConfig.Value;
        }

        public int TokenMaxMinute => _jwtConfig.TokenDurationInSeconds;

        public TokenDTO GenerateAccessTokenFromClaims(params Claim[] claims)
        {
            var issued = Clock.Now;
            var expires = Clock.Now.AddSeconds(_jwtConfig.TokenDurationInSeconds);

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtConfig.SecurityKey));

            var jwtToken = new JwtSecurityToken(issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                notBefore: issued,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new TokenDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                RefreshToken = GenerateRefreshToken(),
                Expires = expires
            };
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtConfig.SecurityKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}