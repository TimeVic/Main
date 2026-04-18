using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.List.Currency;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.List.Currency;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/list/currency/list";
    
    private readonly UserEntity _user;
    private new readonly IDataFactory<PaymentEntity> _factory;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly IPaymentSeeder _paymentSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _factory = ServiceProvider.GetRequiredService<IDataFactory<PaymentEntity>>();
        _paymentSeeder = ServiceProvider.GetRequiredService<IPaymentSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest(){});
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest(){});
        
        // Assert
        await response.EnsureSuccessStatusCodeWithoutError();

        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(154, actualResponse.Count);
        
        Assert.All(actualResponse, item =>
        {
            Assert.NotNull(item.Code);
            Assert.NotNull(item.Symbol);
        });
    }
}
