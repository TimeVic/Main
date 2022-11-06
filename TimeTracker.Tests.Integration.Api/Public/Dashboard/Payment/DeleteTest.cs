using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.Payment;

public class DeleteTest: BaseTest
{
    private readonly string Url = "/dashboard/payment/delete";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly ClientEntity _client;
    private readonly ProjectEntity _project;
    private readonly IPaymentSeeder _paymentSeeder;
    private readonly PaymentEntity _payment;
    private readonly IPaymentDao _paymentDao;
    private WorkspaceEntity _workspace;

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _paymentDao = ServiceProvider.GetRequiredService<IPaymentDao>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IPaymentSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _payment = _paymentSeeder.CreateSeveralAsync(_user, 1).Result.First();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new DeleteRequest()
        {
            PaymentId = _payment.Id,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldDelete()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            PaymentId = _payment.Id
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
            PaymentId = _payment.Id
        });
        var errorResponse = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.Type);
        
        var payment = await _paymentDao.GetById(_payment.Id);
        Assert.NotNull(payment);
    }
}
