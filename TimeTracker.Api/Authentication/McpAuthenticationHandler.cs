using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Api.Authentication;

public class McpAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "McpBearer";

    private readonly IJwtAuthService _jwtAuthService;

    public McpAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IJwtAuthService jwtAuthService
    ) : base(options, logger, encoder)
    {
        _jwtAuthService = jwtAuthService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Request.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var decoded = _jwtAuthService.DecodeMcpJwt(token);
        if (decoded == null)
        var userId = _jwtAuthService.DecodeMcpJwt(token);
        if (!userId.HasValue)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid or expired MCP JWT"));
        }

        var (userId, workspaceId) = decoded.Value;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(AuthConstants.McpWorkspaceIdClaimType, workspaceId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.Value.ToString()),
            new(AuthConstants.McpPurposeClaimType, AuthConstants.McpTokenPurpose),
            new(ClaimTypes.Role, "mcp_client")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
