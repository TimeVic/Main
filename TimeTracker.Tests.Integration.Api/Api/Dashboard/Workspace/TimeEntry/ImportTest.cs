using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace.TimeEntry;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Common.Constants.Import;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Workspace.TimeEntry;

public class ImportTest : BaseTest
{
    private const string Url = "/dashboard/workspace/time-entry/import";

    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly string _jwtToken;
    private readonly ITimeEntryDao _timeEntryDao;

    public ImportTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            data: new Dictionary<string, object>
            {
                { "SourceType", TimeEntryImportSourceType.Clockify },
                { "IsBillable", true }
            },
            file: CreateFormFile("clockify_export_all_cases.csv")
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotImportIfHasNoAccessToWorkspace()
    {
        var (otherToken, _, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();

        var response = await PostMultipartFormDataRequestAsync(
            Url,
            otherToken,
            new Dictionary<string, object>
            {
                { "SourceType", TimeEntryImportSourceType.Clockify },
                { "IsBillable", true }
            },
            CreateFormFile("clockify_export_all_cases.csv"),
            workspaceId: _workspace.Id
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task ShouldImportClockifyCsv()
    {
        var fileToUpload = CreateFormFile("clockify_export_all_cases.csv");
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>
            {
                { "SourceType", TimeEntryImportSourceType.Clockify },
                { "IsBillable", true },
                { "HourlyRate", 50.0 }
            },
            fileToUpload
        );
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<ImportResponse>();
        Assert.Equal(6, actualData.TotalCount);
        Assert.Equal(5, actualData.ImportedCount);
        Assert.Equal(1, actualData.SkippedCount);

        await FlushDbChanges(true);
        var entries = await _timeEntryDao.GetListAsync(_workspace, 1, user: _user);
        Assert.Equal(5, entries.TotalCount);
    }

    [Fact]
    public async Task ShouldSkipDuplicatesOnReimport()
    {
        // First import
        var response1 = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>
            {
                { "SourceType", TimeEntryImportSourceType.Clockify },
                { "IsBillable", true }
            },
            CreateFormFile("clockify_export_all_cases.csv")
        );
        response1.EnsureSuccessStatusCode();
        var data1 = await response1.GetJsonDataAsync<ImportResponse>();
        Assert.Equal(5, data1.ImportedCount);

        await FlushDbChanges(true);

        // Second import of the same file
        var response2 = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>
            {
                { "SourceType", TimeEntryImportSourceType.Clockify },
                { "IsBillable", true }
            },
            CreateFormFile("clockify_export_all_cases.csv")
        );
        response2.EnsureSuccessStatusCode();
        var data2 = await response2.GetJsonDataAsync<ImportResponse>();
        Assert.Equal(0, data2.ImportedCount);
        Assert.Equal(6, data2.SkippedCount);
    }

    [Fact]
    public async Task ShouldReturnErrorIfEmptyFile()
    {
        var emptyFile = new FormFile(new MemoryStream(), 0, 0, "file", "empty.csv");
        var response = await PostMultipartFormDataRequestAsync(
            Url,
            _jwtToken,
            new Dictionary<string, object>
            {
                { "SourceType", TimeEntryImportSourceType.Clockify },
                { "IsBillable", true }
            },
            emptyFile
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
