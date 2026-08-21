using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Import.TimeEntry;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.TimeEntry.Actions;

public class ImportRequestHandler : IAsyncRequestHandler<ImportRequest, ImportResponse>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly ISecurityManager _securityManager;
    private readonly ITimeEntryImportService _timeEntryImportService;

    public ImportRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        ITimeEntryImportService timeEntryImportService
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _securityManager = securityManager;
        _timeEntryImportService = timeEntryImportService;
    }

    public async Task<ImportResponse> ExecuteAsync(ImportRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            throw new IncorrectFileException("File was not provided or is empty");
        }

        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
        RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");

        if (!await _securityManager.HasAccess(AccessLevel.Write, user, workspace))
        {
            throw new HasNoAccessException();
        }

        await using var stream = request.File.OpenReadStream();
        var result = await _timeEntryImportService.ImportAsync(
            user,
            workspace,
            stream,
            request.SourceType,
            request.IsBillable,
            request.HourlyRate
        );

        return new ImportResponse
        {
            ImportedCount = result.ImportedCount,
            SkippedCount = result.SkippedCount,
            TotalCount = result.TotalCount
        };
    }
}
