using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace TimeTracker.Business.Services.Auth
{
    public class JwtAuthService: IJwtAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IJwtAuthService> _logger;

        private readonly string _issuer;
        private readonly string _audience;
        private readonly SymmetricSecurityKey _key;
        
        public JwtAuthService(
            IConfiguration configuration,
            ILogger<IJwtAuthService> logger
        )
        {
            _configuration = configuration;
            _logger = logger;
            _key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration.GetValue<string>("App:Auth:SymmetricSecurityKey")
                )
            );
            _issuer = _configuration.GetValue<string>("App:Auth:Issuer");
            _audience = _configuration.GetValue<string>("App:Auth:Audience");
        }

        public string BuildJwt(long userId)
        {
            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, "user"),
                new(ClaimsIdentity.DefaultRoleClaimType, "user"),
                new(ClaimTypes.NameIdentifier, userId.ToString())
            };
            
            var now = DateTime.UtcNow;
            var expirationTime = now.Add(TimeSpan.FromHours(
                _configuration.GetValue<int>("App:Auth:Lifetime")
            ));
            var signingCredentials =
                new SigningCredentials(
                    _key, 
                    SecurityAlgorithms.HmacSha256
                );
            var jwt = new JwtSecurityToken(
                _issuer,
                _audience,
                notBefore: now,
                claims: claims,
                expires: expirationTime,
                signingCredentials: signingCredentials
            );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        
        public long GetUserId(string jwtString)
        {
            jwtString = jwtString ?? throw new ArgumentNullException(nameof(jwtString));
            try
            {   
                var jwt = new JwtSecurityToken(jwtString);
                return long.Parse(jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            }
            catch (Exception)
            {
                return 0;
            }
        }
        
        public bool IsValidJwt(string token)
        {
            token = token ?? throw new ArgumentNullException(nameof(token));
            var parameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = _key
            };
            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(
                    token,
                    parameters,
                    out SecurityToken validatedToken
                );
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Jwt Auth Token is Incorrect: ${e.Message}", e);
                return false;
            }
            return true;
        }
    }
}
