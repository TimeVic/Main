using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace TimeTracker.Business.Services.Auth
{
    public class JwtAuthService : IJwtAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IJwtAuthService> _logger;

        private readonly string _issuer;
        private readonly string _audience;
        private readonly SymmetricSecurityKey _key;
        private readonly int _lifeTime;

        public JwtAuthService(
            IConfiguration configuration,
            ILogger<IJwtAuthService> logger
        )
        {
            _configuration = configuration;
            _logger = logger;
            _key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration.GetValue<string>("App:Auth:SymmetricSecurityKey")!
                )
            );
            _issuer = _configuration.GetValue<string>("App:Auth:Issuer")!;
            _audience = _configuration.GetValue<string>("App:Auth:Audience")!;
            _lifeTime = _configuration.GetValue<int>("App:Auth:JwtLifetime")!;
        }

        public string BuildJwt(
            Guid userId,
            Guid? accessTokenId = null,
            DateTime? expirationTime = null,
            DateTime? notBeforeTime = null    
        )
        {
            var now = DateTime.UtcNow;
            expirationTime ??= now.Add(TimeSpan.FromMinutes(_lifeTime));
            notBeforeTime ??= now;
            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, "user"),
                new(ClaimsIdentity.DefaultRoleClaimType, "user"),
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                
                new(ClaimTypes.Authentication, accessTokenId?.ToString() ?? string.Empty)
            };
            var signingCredentials = new SigningCredentials(
                _key,
                SecurityAlgorithms.HmacSha256
            );
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _issuer,
                Audience = _audience,
                NotBefore = notBeforeTime,
                Subject = new ClaimsIdentity(claims),
                Expires = expirationTime,
                SigningCredentials = signingCredentials
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenObject = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(tokenObject);
        }

        public Guid GetUserId(string jwtString)
        {
            jwtString = jwtString ?? throw new ArgumentNullException(nameof(jwtString));
            try
            {
                var jwt = new JwtSecurityToken(jwtString);
                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid");
                ArgumentNullException.ThrowIfNull(userIdClaim);
                return Guid.Parse(userIdClaim.Value);
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        public bool IsValidJwt(string token, bool isValidateLifeTime = true)
        {
            token = token ?? throw new ArgumentNullException(nameof(token));
            var parameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = isValidateLifeTime,
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
        
        public bool IsJwt(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            try
            {
                var jwt = jwtHandler.ReadJwtToken(token);
                return jwt != null;
            }
            catch
            {
                return false;
            }
        }
        
        public Guid? GetAccessTokenId(string jwtString)
        {
            return GetClaimValue<Guid>(ClaimTypes.Authentication, jwtString);
        }
        
        public bool IsTokenExpired(string token, TimeSpan? delayBefore = null)
        {
            var expirationTime = GetTokenExpirationTime(token);
            if (delayBefore != null)
            {
                expirationTime = expirationTime.Add(-delayBefore.Value);
            }
            return expirationTime < DateTime.UtcNow;
        }
        
        public DateTime GetTokenExpirationTime(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            if (!jwtHandler.CanReadToken(token))
                throw new ArgumentException("Invalid JWT token");

            var jwtToken = jwtHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        
        private T? GetClaimValue<T>(string claimType, string jwtString)
        {
            jwtString = jwtString ?? throw new ArgumentNullException(nameof(jwtString));
            try
            {   
                var jwt = new JwtSecurityToken(jwtString);
                var value = jwt.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
                if (value == null)
                    return default;
                if (typeof(T) == typeof(Guid))
                {
                    if (Guid.TryParse(value, out var guidValue))
                    {
                        return (T)Convert.ChangeType(guidValue, typeof(T));
                    }
                }
                if (typeof(T) == typeof(bool))
                {
                    if (bool.TryParse(value, out var decimalValue))
                    {
                        return (T)Convert.ChangeType(decimalValue, typeof(T));
                    }
                }
                if (typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public string BuildMcpJwt(
            Guid userId,
            Guid workspaceId,
            DateTime? expirationTime = null
        )
        {
            var now = DateTime.UtcNow;
            expirationTime ??= now.Add(TimeSpan.FromMinutes(_lifeTime));
            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, "mcp_user"),
                new(ClaimsIdentity.DefaultRoleClaimType, "mcp_client"),
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(TimeTracker.Business.Common.Constants.Http.AuthConstants.McpWorkspaceIdClaimType, workspaceId.ToString()),
                new(TimeTracker.Business.Common.Constants.Http.AuthConstants.McpPurposeClaimType, TimeTracker.Business.Common.Constants.Http.AuthConstants.McpTokenPurpose)
            };
            var signingCredentials = new SigningCredentials(
                _key,
                SecurityAlgorithms.HmacSha256
            );
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _issuer,
                Audience = _audience,
                NotBefore = now,
                Subject = new ClaimsIdentity(claims),
                Expires = expirationTime,
                SigningCredentials = signingCredentials
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenObject = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(tokenObject);
        }

        public (Guid UserId, Guid WorkspaceId)? DecodeMcpJwt(string token)
        public Guid? DecodeMcpJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = _key,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, parameters, out _);
                
                var purpose = principal.FindFirst(TimeTracker.Business.Common.Constants.Http.AuthConstants.McpPurposeClaimType)?.Value;
                if (purpose != TimeTracker.Business.Common.Constants.Http.AuthConstants.McpTokenPurpose)
                {
                    return null;
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var workspaceIdClaim = principal.FindFirst(TimeTracker.Business.Common.Constants.Http.AuthConstants.McpWorkspaceIdClaimType)?.Value;

                if (Guid.TryParse(userIdClaim, out var userId) && Guid.TryParse(workspaceIdClaim, out var workspaceId))
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    return (userId, workspaceId);
                    return userId;
                }

                return null;
            }
            catch (Exception e)
            {
                _logger.LogDebug($"MCP Jwt Auth Token is Incorrect: {e.Message}", e);
                return null;
            }
        }
    }
}
