using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.MemberPayment;

public class AddTest: BaseTest
{
    private readonly string Url = "/dashboard/member-payments/add";
    
    private readonly UserEntity _user;
    private new readonly IDataFactory<MemberPaymentEntity> _factory;
    private readonly string _jwtToken;
    private readonly IClientDao _clientDao;
    private readonly WorkspaceEntity _workspace;
    private readonly ClientEntity _client;
    private readonly IProjectDao _projectDao;
    private readonly ProjectEntity _project;
    private readonly IUserSeeder _userSeeder;

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _factory = ServiceProvider.GetRequiredService<IDataFactory<MemberPaymentEntity>>();
        _clientDao = ServiceProvider.GetRequiredService<IClientDao>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _client = _clientDao.CreateAsync(_workspace, "Test adding").Result;
        _project = _projectDao.CreateAsync(_workspace, "Test adding").Result;
        _project.SetClient(_client);
        FlushDbChanges().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var payment = _factory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new AddRequest()
        {
            WorkspaceId = _workspace.Id,
            ClientId = _client.Id,
            Amount = payment.Amount,
            Description = payment.Description,
            PaymentTime = DateTime.Now,
            ProjectId = _project.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldAdd()
    {
        var expectedPaymentTime = DateTime.Now;
        var payment = _factory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            WorkspaceId = _workspace.Id,
            ClientId = _client.Id,
            Amount = payment.Amount,
            Description = payment.Description,
            PaymentTime = expectedPaymentTime,
            ProjectId = _project.Id
        });
        response.EnsureSuccessStatusCode();

        var actualMemberPayment = await response.GetJsonDataAsync<MemberPaymentDto>();
        Assert.NotNull(actualMemberPayment.Project);
        Assert.NotEqual(Guid.Empty, actualMemberPayment.Id);
        Assert.Equal(_client.Id, actualMemberPayment.Client.Id);
        Assert.Equal(_project.Id, actualMemberPayment.Project.Id);
        Assert.Equal(payment.Amount, actualMemberPayment.Amount);
        Assert.Equal(payment.Description, actualMemberPayment.Description);
        Assert.Equal(expectedPaymentTime.ToUniversalTime(), actualMemberPayment.PaymentTime);
    }
    
    [Fact]
    public async Task UserWithRoleUserCanAddOwnMemberPayment()
    {
        var (otherToken, otherUser, otherWorkspace) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        
        var payment = _factory.Generate();
        var response = await PostRequestAsync(Url, otherToken, new AddRequest()
        {
            WorkspaceId = _workspace.Id,
            ClientId = _client.Id,
            Amount = payment.Amount,
            Description = payment.Description,
            PaymentTime = payment.PaymentTime
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualMemberPayment = await response.GetJsonDataAsync<MemberPaymentDto>();
        Assert.NotEqual(Guid.Empty, actualMemberPayment.Id);
    }
    
    [Fact]
    public async Task UserWithRoleManagerCanAddOwnMemberPayment()
    {
        var (otherToken, otherUser, otherWorkspace) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.Manager
        );
        
        var payment = _factory.Generate();
        var response = await PostRequestAsync(Url, otherToken, new AddRequest()
        {
            WorkspaceId = _workspace.Id,
            ClientId = _client.Id,
            Amount = payment.Amount,
            Description = payment.Description,
            PaymentTime = payment.PaymentTime
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualMemberPayment = await response.GetJsonDataAsync<MemberPaymentDto>();
        Assert.NotEqual(Guid.Empty, actualMemberPayment.Id);
    }
    
    [Fact]
    public async Task NonMemberCantAddOwnMemberPayment()
    {
        var (otherToken, otherUser, otherWorkspace) = await _userSeeder.CreateAuthorizedAsync();
        
        var payment = _factory.Generate();
        var response = await PostRequestAsync(Url, otherToken, new AddRequest()
        {
            WorkspaceId = _workspace.Id,
            ClientId = _client.Id,
            Amount = payment.Amount,
            Description = payment.Description,
            PaymentTime = payment.PaymentTime
        });
        
        var responseData = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), responseData.ErrorCode);
    }
}
