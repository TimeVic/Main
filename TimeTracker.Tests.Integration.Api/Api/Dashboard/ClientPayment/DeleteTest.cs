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

public class DeleteTest: BaseTest
{
    private const string Url = "/dashboard/client-payments/delete";

    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly IClientPaymentSeeder _paymentSeeder;
    private readonly ClientPaymentEntity _payment;
    private readonly IClientPaymentDao _paymentDao;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserSeeder _userSeeder;

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _paymentDao = ServiceProvider.GetRequiredService<IClientPaymentDao>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IClientPaymentSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _payment = _paymentSeeder.CreateSeveralAsync(_workspace, 1).Result.First();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new DeleteRequest()
        {
            ClientPaymentId = _payment.Id,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldDeleteIfWorkspaceOwner()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            ClientPaymentId = _payment.Id
        });
        response.EnsureSuccessStatusCode();

        var payment = await _paymentDao.GetById(_payment.Id);
        Assert.Null(payment);
    }

    [Fact]
    public async Task ShouldDeleteIfWorkspaceManager()
    {
        var payment = (await _paymentSeeder.CreateSeveralAsync(_workspace, 1)).First();
        var (otherToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.Manager
        );

        var response = await PostRequestAsync(Url, otherToken, new DeleteRequest()
        {
            ClientPaymentId = payment.Id
        });
        response.EnsureSuccessStatusCode();

        var actualPayment = await _paymentDao.GetById(payment.Id);
        Assert.Null(actualPayment);
    }

    [Fact]
    public async Task ShouldNotDeleteIfWorkspaceUser()
    {
        var (otherToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        var response = await PostRequestAsync(Url, otherToken, new DeleteRequest()
        {
            ClientPaymentId = _payment.Id
        });
        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.ErrorCode);

        var payment = await _paymentDao.GetById(_payment.Id);
        Assert.NotNull(payment);
    }
}
