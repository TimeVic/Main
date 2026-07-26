using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;
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

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.ClientPayment;

public class UpdateTest: BaseTest
{
    private const string Url = "/dashboard/client-payments/update";

    private readonly UserEntity _user;
    private new readonly IDataFactory<ClientPaymentEntity> _factory;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IClientPaymentSeeder _paymentSeeder;
    private readonly ClientPaymentEntity _payment;
    private readonly IUserSeeder _userSeeder;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _factory = ServiceProvider.GetRequiredService<IDataFactory<ClientPaymentEntity>>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IClientPaymentSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        var project = _projectSeeder.CreateAsync(_workspace).Result;
        _payment = _paymentSeeder.CreateSeveralAsync(project.Client, project, 1).Result.First();
        FlushDbChanges().Wait();

        Assert.NotNull(_payment.Project);
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var expectPayment = _factory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            ClientPaymentId = _payment.Id,
            ClientId = _payment.Client.Id,
            Amount = expectPayment.Amount,
            Description = expectPayment.Description,
            PaymentTime = expectPayment.PaymentTime,
            ProjectId = _payment.Project!.Id,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldUpdateIfWorkspaceOwner()
    {
        var expectPayment = _factory.Generate();
        var expectProject = await _projectSeeder.CreateAsync(_workspace);
        var expectedClient = expectProject.Client;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            ClientPaymentId = _payment.Id,
            ClientId = expectedClient.Id,
            Amount = expectPayment.Amount,
            Description = expectPayment.Description,
            PaymentTime = expectPayment.PaymentTime,
            ProjectId = expectProject.Id
        });
        response.EnsureSuccessStatusCode();

        var actualPayment = await response.GetJsonDataAsync<ClientPaymentDto>();
        Assert.NotNull(actualPayment.Project);
        Assert.NotEqual(Guid.Empty, actualPayment.Id);
        Assert.Equal(expectedClient.Id, actualPayment.Client.Id);
        Assert.Equal(expectProject.Id, actualPayment.Project.Id);
        Assert.Equal(expectPayment.Amount, actualPayment.Amount);
        Assert.Equal(expectPayment.Description, actualPayment.Description);
        Assert.Equal(expectPayment.PaymentTime, actualPayment.PaymentTime.ToUniversalTime());
    }

    [Fact]
    public async Task ShouldUpdateIfWorkspaceManager()
    {
        var (otherToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.Manager
        );
        var expectPayment = _factory.Generate();

        var response = await PostRequestAsync(Url, otherToken, new UpdateRequest()
        {
            ClientPaymentId = _payment.Id,
            ClientId = _payment.Client.Id,
            Amount = expectPayment.Amount,
            Description = expectPayment.Description,
            PaymentTime = expectPayment.PaymentTime,
            ProjectId = _payment.Project!.Id
        });

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ShouldNotUpdateIfWorkspaceUser()
    {
        var (otherToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        var expectPayment = _factory.Generate();

        var response = await PostRequestAsync(Url, otherToken, new UpdateRequest()
        {
            ClientPaymentId = _payment.Id,
            ClientId = _payment.Client.Id,
            Amount = expectPayment.Amount,
            Description = expectPayment.Description,
            PaymentTime = expectPayment.PaymentTime,
            ProjectId = _payment.Project!.Id
        });

        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.ErrorCode);
    }

    [Fact]
    public async Task ShouldNotUpdatePaymentForSoftDeletedProject()
    {
        _payment.Project!.DeletedAt = DateTime.UtcNow;
        await FlushDbChanges();
        var expectedPayment = _factory.Generate();

        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest
        {
            ClientPaymentId = _payment.Id,
            ClientId = _payment.Client.Id,
            Amount = expectedPayment.Amount,
            Description = expectedPayment.Description,
            PaymentTime = expectedPayment.PaymentTime,
            ProjectId = _payment.Project.Id
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), error.ErrorCode);
    }
}
