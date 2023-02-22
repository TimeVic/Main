using System.Drawing;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tag;

public class DeleteTest: BaseTest
{
    private readonly string Url = "/dashboard/tag/delete";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<TagEntity> _tagFactory;
    private readonly string _jwtToken;
    private readonly ITagSeeder _tagSeeder;
    private readonly TagEntity _tag;
    private readonly IClientSeeder _clientSeeder;
    private readonly WorkspaceEntity _workspace;
    private readonly Color _expectedColor = Color.Blue;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITimeEntrySeeder _timeEntrySeeder;

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _tagFactory = ServiceProvider.GetRequiredService<IDataFactory<TagEntity>>();
        _tagSeeder = ServiceProvider.GetRequiredService<ITagSeeder>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _clientSeeder = ServiceProvider.GetRequiredService<IClientSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _tag = _tagSeeder.CreateAsync(_workspace).Result;

    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var expectedProject = _tagFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new DeleteRequest()
        {
            TagId = _tag.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldDelete()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        task.Tags.Add(_tag);
        var timeEntries = await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 1);
        var timeEntry = timeEntries.First();
        timeEntry.Tags.Add(_tag);
        await DbSessionProvider.PerformCommitAsync();
        
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            TagId = _tag.Id
        });
        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task ShouldNotDeleteIfHasNoAccess()
    {
        var (otherJwtToken, _, _) = await UserSeeder.CreateAuthorizedAsync();
        await DbSessionProvider.PerformCommitAsync();
        
        var response = await PostRequestAsync(Url, otherJwtToken, new DeleteRequest()
        {
            TagId = _tag.Id
        });
        
        var errorResponse = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.Type);
    }
}
