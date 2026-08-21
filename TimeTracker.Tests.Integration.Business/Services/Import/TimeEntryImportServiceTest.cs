using System.Text;
using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Import;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Import.TimeEntry;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Import;

public class TimeEntryImportServiceTest : BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IUserDao _userDao;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITimeEntryImportService _importService;
    private readonly ITagDao _tagDao;

    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;

    public TimeEntryImportServiceTest() : base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _userDao = Scope.Resolve<IUserDao>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _importService = Scope.Resolve<ITimeEntryImportService>();
        _tagDao = Scope.Resolve<ITagDao>();

        var (_, user, workspace) = _userSeeder.CreateAuthorizedAsync().Result;
        _user = user;
        _workspace = workspace;
    }

    private static Stream GetStubStream(string fileName)
    {
        var stubsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stubs", fileName);
        return File.OpenRead(stubsPath);
    }

    private static MemoryStream CreateCsvStream(string csvContent)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
    }

    [Fact]
    public async Task ShouldImportClockifyCsvSuccessfully()
    {
        using var stream = GetStubStream("clockify_export_all_cases.csv");

        var result = await _importService.ImportAsync(
            _user,
            _workspace,
            stream,
            TimeEntryImportSourceType.Clockify,
            defaultIsBillable: true,
            defaultHourlyRate: 75.5m
        );

        Assert.Equal(6, result.TotalCount);
        Assert.Equal(5, result.ImportedCount);
        Assert.Equal(1, result.SkippedCount); // duplicate row skipped

        await FlushDbChanges();

        var listDto = await _timeEntryDao.GetListAsync(_workspace, 1, user: _user);
        Assert.Equal(5, listDto.TotalCount);
    }

    [Fact]
    public async Task ShouldHandleMissingClientByCreatingSourceNamedClient()
    {
        const string csv = """"
"Project","Client","Description","Task","User","Group","Email","Tags","Start Date","Start Time","End Date","End Time"
"Backend Dev","","Refactoring","","Test User","","test@test.com","","04/11/2026","10:00","04/11/2026","11:00"
"""";
        using var stream = CreateCsvStream(csv);

        var result = await _importService.ImportAsync(
            _user,
            _workspace,
            stream,
            TimeEntryImportSourceType.Clockify,
            defaultIsBillable: false,
            defaultHourlyRate: null
        );

        Assert.Equal(1, result.ImportedCount);
        await FlushDbChanges();

        var list = await _timeEntryDao.GetListAsync(_workspace, 1, user: _user);
        var entry = list.Items.First();
        Assert.NotNull(entry.Project);
        Assert.Equal("Backend Dev", entry.Project.Name);
        Assert.NotNull(entry.Project.Client);
        Assert.Equal("Clockify", entry.Project.Client.Name);
    }

    [Fact]
    public async Task ShouldHandleMissingProjectByCreatingDefaultProjectForClient()
    {
        const string csv = """"
"Project","Client","Description","Task","User","Group","Email","Tags","Start Date","Start Time","End Date","End Time"
"","Acme Corp","Support consultation","","Test User","","test@test.com","","04/11/2026","10:00","04/11/2026","11:00"
"""";
        using var stream = CreateCsvStream(csv);

        var result = await _importService.ImportAsync(
            _user,
            _workspace,
            stream,
            TimeEntryImportSourceType.Clockify,
            defaultIsBillable: false,
            defaultHourlyRate: null
        );

        Assert.Equal(1, result.ImportedCount);
        await FlushDbChanges();

        var list = await _timeEntryDao.GetListAsync(_workspace, 1, user: _user);
        var entry = list.Items.First();
        Assert.NotNull(entry.Project);
        Assert.Equal("Default", entry.Project.Name);
        Assert.NotNull(entry.Project.Client);
        Assert.Equal("Acme Corp", entry.Project.Client.Name);
    }

    [Fact]
    public async Task ShouldHandleMissingBothClientAndProject()
    {
        const string csv = """"
"Project","Client","Description","Task","User","Group","Email","Tags","Start Date","Start Time","End Date","End Time"
"","","Standalone task","","Test User","","test@test.com","","04/11/2026","10:00","04/11/2026","11:00"
"""";
        using var stream = CreateCsvStream(csv);

        var result = await _importService.ImportAsync(
            _user,
            _workspace,
            stream,
            TimeEntryImportSourceType.Clockify,
            defaultIsBillable: false,
            defaultHourlyRate: null
        );

        Assert.Equal(1, result.ImportedCount);
        await FlushDbChanges();

        var list = await _timeEntryDao.GetListAsync(_workspace, 1, user: _user);
        var entry = list.Items.First();
        Assert.Null(entry.Project);
        Assert.Equal("Standalone task", entry.Description);
    }

    [Fact]
    public async Task ShouldSkipDuplicateEntriesOnReimport()
    {
        using (var stream1 = GetStubStream("clockify_export_all_cases.csv"))
        {
            var result1 = await _importService.ImportAsync(
                _user,
                _workspace,
                stream1,
                TimeEntryImportSourceType.Clockify,
                defaultIsBillable: true,
                defaultHourlyRate: 50m
            );
            Assert.Equal(5, result1.ImportedCount);
        }

        await FlushDbChanges();

        using (var stream2 = GetStubStream("clockify_export_all_cases.csv"))
        {
            var result2 = await _importService.ImportAsync(
                _user,
                _workspace,
                stream2,
                TimeEntryImportSourceType.Clockify,
                defaultIsBillable: true,
                defaultHourlyRate: 50m
            );
            Assert.Equal(0, result2.ImportedCount);
            Assert.Equal(6, result2.SkippedCount);
            Assert.Equal(6, result2.TotalCount);
        }
    }

    [Fact]
    public async Task ShouldCreateAndLinkTags()
    {
        const string csv = """"
"Project","Client","Description","Task","User","Group","Email","Tags","Start Date","Start Time","End Date","End Time"
"App Dev","Client A","Feature work","","Test User","","test@test.com","Frontend, High Priority","04/11/2026","10:00","04/11/2026","11:00"
"""";
        using var stream = CreateCsvStream(csv);

        var result = await _importService.ImportAsync(
            _user,
            _workspace,
            stream,
            TimeEntryImportSourceType.Clockify,
            defaultIsBillable: true,
            defaultHourlyRate: null
        );

        Assert.Equal(1, result.ImportedCount);
        await FlushDbChanges();

        var workspaceTags = await _tagDao.GetList(_workspace);
        Assert.Contains(workspaceTags, t => t.Name == "Frontend");
        Assert.Contains(workspaceTags, t => t.Name == "High Priority");

        var list = await _timeEntryDao.GetListAsync(_workspace, 1, user: _user);
        var entry = list.Items.First();
        Assert.Equal(2, entry.Tags.Count);
    }

    [Fact]
    public async Task ShouldRespectCustomBillableAndHourlyRate()
    {
        const string csv = """"
"Project","Client","Description","Task","User","Group","Email","Tags","Start Date","Start Time","End Date","End Time"
"Proj","Client","Work","","User","","u@u.com","","04/11/2026","10:00","04/11/2026","11:00"
"""";
        using var stream = CreateCsvStream(csv);

        var result = await _importService.ImportAsync(
            _user,
            _workspace,
            stream,
            TimeEntryImportSourceType.Clockify,
            defaultIsBillable: true,
            defaultHourlyRate: 150.25m
        );

        Assert.Equal(1, result.ImportedCount);
        await FlushDbChanges();

        var list = await _timeEntryDao.GetListAsync(_workspace, 1, user: _user);
        var entry = list.Items.First();
        Assert.True(entry.IsBillable);
        Assert.Equal(150.25m, entry.HourlyRate);
    }

    [Fact]
    public async Task ShouldConvertLocalTimestampsToUtcAccordingToWorkspaceTimeZone()
    {
        _workspace.TimeZone = "Europe/Kyiv"; // UTC+3 during DST / UTC+2 standard
        await DbSessionProvider.CurrentSession.SaveAsync(_workspace);
        await FlushDbChanges();

        const string csv = """"
"Project","Client","Description","Task","User","Group","Email","Tags","Start Date","Start Time","End Date","End Time"
"Proj","Client","Work","","User","","u@u.com","","07/15/2026","15:00","07/15/2026","16:00"
"""";
        using var stream = CreateCsvStream(csv);

        var result = await _importService.ImportAsync(
            _user,
            _workspace,
            stream,
            TimeEntryImportSourceType.Clockify,
            defaultIsBillable: false,
            defaultHourlyRate: null
        );

        Assert.Equal(1, result.ImportedCount);
        await FlushDbChanges();

        var list = await _timeEntryDao.GetListAsync(_workspace, 1, user: _user);
        var entry = list.Items.First();

        var kyivTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        var expectedStartUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2026, 7, 15, 15, 0, 0), kyivTz);
        var expectedEndUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2026, 7, 15, 16, 0, 0), kyivTz);

        Assert.Equal(expectedStartUtc, entry.StartTime);
        Assert.Equal(expectedEndUtc, entry.EndTime);
    }

    [Fact]
    public async Task ShouldSkipInvalidRowsGracefully()
    {
        using var stream = GetStubStream("clockify_export_with_invalid_rows.csv");

        var result = await _importService.ImportAsync(
            _user,
            _workspace,
            stream,
            TimeEntryImportSourceType.Clockify,
            defaultIsBillable: false,
            defaultHourlyRate: null
        );

        Assert.Equal(2, result.ImportedCount);
        Assert.Equal(0, result.SkippedCount); // only valid parsed rows are in TotalCount (2 rows)
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ShouldThrowIfMissingRequiredClockifyHeaders()
    {
        using var stream = GetStubStream("clockify_export_missing_headers.csv");

        await Assert.ThrowsAsync<IncorrectFileException>(async () =>
        {
            await _importService.ImportAsync(
                _user,
                _workspace,
                stream,
                TimeEntryImportSourceType.Clockify,
                defaultIsBillable: false,
                defaultHourlyRate: null
            );
        });
    }
}
