using System.Security.Claims;
using GP_Server.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GP_Server.Application.Services;

public class TokenService
{
    private readonly string _key;
        private readonly string _issuer;
        private readonly int _expiryInHours;

        public TokenService(string? key, string? issuer, int expiryInHours)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
            _expiryInHours = expiryInHours;
        }

        public string GenerateToken(IList<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _issuer,
                _issuer,
                claims,
                expires: DateTime.Now.AddHours(_expiryInHours),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
}
