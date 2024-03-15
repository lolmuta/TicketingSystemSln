using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TicketingSystem.LoginUtil
{
    public class JwtService
    {
        private readonly IConfiguration configuration;

        private readonly string SecretKey;// = "yourSecretKeyaaaaaaaaaaaaaaaaaaaaaaa";
        private readonly string Issuer;// = "yourIssuer";
        private readonly string Audience;// = "yourAudience";
        private readonly double ExpirationHours;// = 1;
        public JwtService(IConfiguration configuration)
        {
            this.configuration = configuration;
            SecretKey = configuration.GetValue<string>("JwtAuth:SecretKey");
            Issuer = configuration.GetValue<string>("JwtAuth:Issuer");
            Audience = configuration.GetValue<string>("JwtAuth:Audience");
            ExpirationHours = configuration.GetValue<double>("JwtAuth:ExpirationHours");
        }
        public string GenerateJwtToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, username)
                }),
                Expires = DateTime.UtcNow.AddHours(ExpirationHours),
                Issuer = Issuer,
                Audience = Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(SecretKey);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                var identity = principal.Identity as ClaimsIdentity;
                return identity?.FindFirst(ClaimTypes.Name)?.Value;
            }
            catch (Exception)
            {
                // Token is invalid or expired
                return null;
            }
        }
    }
}
