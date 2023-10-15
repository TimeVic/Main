using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.User;

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
        await response.GetJsonDataAsync<RefreshTokenResponseDto>();
        response.EnsureSuccessStatusCode();
        var responseData = await response.GetJsonDataAsync<RefreshTokenResponseDto>();

        Assert.True(_jwtService.IsValidJwt(responseData.JwtToken));
        Assert.NotEmpty(responseData.AccessToken);
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
        var responseData = await response.GetJsonErrorAsync();
        Assert.Equal(new UserNotAuthorizedException().GetTypeName(), responseData.Type);
    }
}
