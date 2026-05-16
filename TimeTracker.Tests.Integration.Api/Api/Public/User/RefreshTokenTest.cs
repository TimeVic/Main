using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Public.User;

public class RefreshTokenTest: BaseTest
{
    private readonly string Url = "/user/refresh-token";
    
    private readonly IJwtAuthService _jwtService;
    private readonly IAuthorizationService _authorizationService;

    public RefreshTokenTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        _authorizationService = ServiceProvider.GetRequiredService<IAuthorizationService>();
    }

    [Fact]
    public async Task ShouldRefresh()
    {
        var expectedPassword = "test password";
        var user = await UserSeeder.CreateActivatedAsync(expectedPassword);
        var loginResponse = await _authorizationService.Login(user);
        var response = await PostRequestAsAnonymousAsync(Url, new RefreshTokenRequest()
        {
            AccessToken = loginResponse.AccessToken,
            JwtToken = loginResponse.JwtToken
        });
        response.EnsureSuccessStatusCode();
        var responseData = await response.GetJsonDataAsync<RefreshTokenResponseDto>();
        var jwtToken = response.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        var accessToken = response.GetSetCookieValue(HttpCookieKeyEnum.AccessToken.GetKey());

        Assert.True(_jwtService.IsValidJwt(jwtToken!));
        Assert.NotEmpty(accessToken);
        Assert.Empty(responseData.JwtToken);
        Assert.Empty(responseData.AccessToken);
    }
    
    [Fact]
    public async Task ShouldFailIfIncorrectJwt()
    {
        var expectedPassword = "test password";
        var user = await UserSeeder.CreateActivatedAsync(expectedPassword);
        var loginResponse = await _authorizationService.Login(user);
        var response = await PostRequestAsAnonymousAsync(Url, new RefreshTokenRequest()
        {
            AccessToken = loginResponse.AccessToken,
            JwtToken = "aaaaaaaaaaa"
        });
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var responseData = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new UserNotAuthorizedException().GetTypeName(), responseData.ErrorCode);
    }
}
