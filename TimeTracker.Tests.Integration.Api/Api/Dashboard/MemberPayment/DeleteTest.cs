using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.MemberPayment;

public class DeleteTest: BaseTest
{
    private readonly string Url = "/dashboard/member-payments/delete";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly IMemberPaymentSeeder _paymentSeeder;
    private readonly MemberPaymentEntity _payment;
    private readonly IMemberPaymentDao _paymentDao;
    private WorkspaceEntity _workspace;

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _paymentDao = ServiceProvider.GetRequiredService<IMemberPaymentDao>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IMemberPaymentSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspace.Mode = WorkspaceMode.Team;
        DbSessionProvider.CurrentSession.UpdateAsync(_workspace).Wait();

        _payment = _paymentSeeder.CreateSeveralAsync(_user, 1).Result.First();
        _payment.Member.Workspace.Mode = WorkspaceMode.Team;
        DbSessionProvider.CurrentSession.UpdateAsync(_payment.Member.Workspace).Wait();
        FlushDbChanges().Wait();
    }

    [Fact]
    public async Task UserCanNotDeletePaymentInSoloWorkspace()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        _payment.Member.Workspace.Mode = WorkspaceMode.Solo;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            MemberPaymentId = _payment.Id
        });
        
        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.ErrorCode);
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new DeleteRequest()
        {
            MemberPaymentId = _payment.Id,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldDelete()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            MemberPaymentId = _payment.Id
        });
        response.EnsureSuccessStatusCode();

        var payment = await _paymentDao.GetById(_payment.Id);
        Assert.Null(payment);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfHasNoAccess()
    {
        var (otherJwtToken, otherUser, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var response = await PostRequestAsync(Url, otherJwtToken, new DeleteRequest()
        {
            MemberPaymentId = _payment.Id
        });
        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.ErrorCode);
        
        var payment = await _paymentDao.GetById(_payment.Id);
        Assert.NotNull(payment);
    }
}
