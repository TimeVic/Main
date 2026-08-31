using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.User;

public class SearchTest : BaseTest
{
    private const string Url = "/dashboard/user/search";

    private readonly string _jwtToken;
    private readonly UserEntity _user;
    private readonly IUserDao _userDao;

    public SearchTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        (_jwtToken, _user, _) = UserSeeder.CreateAuthorizedAsync().Result;
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new SearchRequest
        {
            Query = "test"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldSearchUsersByLogin()
    {
        var randomSuffix = new Random().Next(10000, 99999).ToString();
        var uniqueLogin = "searchuser_" + randomSuffix;
        var user = await UserSeeder.CreateActivatedAsync();
        await _userDao.ChangeLoginAsync(user, uniqueLogin);
        await FlushDbChanges(isClearSession: true);

        var response = await PostRequestAsync(Url, _jwtToken, new SearchRequest
        {
            Query = "searchuser_" + randomSuffix
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<SearchResponse>();
        Assert.NotNull(actualResponse);
        Assert.NotEmpty(actualResponse.Items);
        Assert.Contains(actualResponse.Items, u => u.Login == uniqueLogin);
    }

    [Fact]
    public async Task ShouldSearchUsersByLoginWithLeadingAt()
    {
        var randomSuffix = new Random().Next(10000, 99999).ToString();
        var uniqueLogin = "atsearch_" + randomSuffix;
        var user = await UserSeeder.CreateActivatedAsync();
        await _userDao.ChangeLoginAsync(user, uniqueLogin);
        await FlushDbChanges(isClearSession: true);

        var response = await PostRequestAsync(Url, _jwtToken, new SearchRequest
        {
            Query = "@" + uniqueLogin
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<SearchResponse>();
        Assert.NotNull(actualResponse);
        Assert.NotEmpty(actualResponse.Items);
        Assert.Contains(actualResponse.Items, u => u.Login == uniqueLogin);
    }

    [Fact]
    public async Task ShouldReturnEmptyIfUserNotFound()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SearchRequest
        {
            Query = "non_existing_user_query_9999999"
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<SearchResponse>();
        Assert.NotNull(actualResponse);
        Assert.Empty(actualResponse.Items);
        Assert.Equal(0, actualResponse.TotalCount);
    }

    [Fact]
    public async Task ShouldReturnBadRequestIfQueryIsEmpty()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SearchRequest
        {
            Query = string.Empty
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
