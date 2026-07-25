using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry;

public class GetActiveTest : BaseTest
{
    private const string Url = "/dashboard/time-entry/get-active";

    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntryDao _timeEntryDao;

    public GetActiveTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetActiveRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnNullWhenNoActiveEntryExists()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new GetActiveRequest());
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetActiveResponse>();

        Assert.Null(actualResponse.ActiveTimeEntry);
    }

    [Fact]
    public async Task ShouldReturnCurrentActiveEntry()
    {
        var expectedEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _defaultWorkspace,
            DateTime.UtcNow.AddMinutes(-1)
        );

        var response = await PostRequestAsync(Url, _jwtToken, new GetActiveRequest());
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetActiveResponse>();

        Assert.NotNull(actualResponse.ActiveTimeEntry);
        Assert.Equal(expectedEntry.Id, actualResponse.ActiveTimeEntry.Id);
        Assert.Null(actualResponse.ActiveTimeEntry.EndTime);
    }
}
