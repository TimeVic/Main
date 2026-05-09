using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.ClientPayment;

public class GetListTest: BaseTest
{
    private const string Url = "/dashboard/client-payments/list";

    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly ClientEntity _client;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ProjectEntity _project;
    private readonly IClientPaymentSeeder _paymentSeeder;
    private readonly IUserSeeder _userSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IClientPaymentSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _project = _projectSeeder.CreateAsync(_workspace).Result;
        _client = _project.Client;
        FlushDbChanges().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            Page = 1
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReceiveListIfWorkspaceOwner()
    {
        var expectedTotal = 21;
        await _paymentSeeder.CreateSeveralAsync(_client, _project, expectedTotal);

        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedTotal, actualResponse.TotalCount);

        Assert.All(actualResponse.Items, item =>
        {
            Assert.NotNull(item.Project);
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.Equal(_client.Id, item.Client.Id);
            Assert.Equal(_project.Id, item.Project.Id);
            Assert.True(item.Amount > 0);
            Assert.NotEmpty(item.Description!);
            Assert.True(item.PaymentTime > DateTime.MinValue);
        });
    }

    [Fact]
    public async Task ShouldReceiveListIfWorkspaceUser()
    {
        var expectedTotal = 3;
        await _paymentSeeder.CreateSeveralAsync(_client, _project, expectedTotal);
        var (otherJwtToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );

        var response = await PostRequestAsync(Url, otherJwtToken, new GetListRequest()
        {
            Page = 1
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedTotal, actualResponse.TotalCount);
    }

    [Fact]
    public async Task ShouldNotReceiveIfHasNoAccess()
    {
        var (otherJwtToken, _, _) = UserSeeder.CreateAuthorizedAsync().Result;

        var response = await PostRequestAsync(Url, otherJwtToken, new GetListRequest()
        {
            Page = 1
        }, _workspace.Id);
        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), errorResponse.ErrorCode);
    }
}
