using System.Security.Cryptography;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.SharedClientReports.Actions;

public class ClientShareReportSettingsRequestHandler : IAsyncRequestHandler<ClientShareReportSettingsRequest, ClientShareReportSettingsResponse>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly IClientDao _clientDao;
    private readonly ISharedClientReportDao _sharedClientReportDao;
    private readonly ISecurityManager _securityManager;
    private readonly IUrlService _urlService;

    public ClientShareReportSettingsRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao,
        IClientDao clientDao,
        ISharedClientReportDao sharedClientReportDao,
        ISecurityManager securityManager,
        IUrlService urlService
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _clientDao = clientDao;
        _sharedClientReportDao = sharedClientReportDao;
        _securityManager = securityManager;
        _urlService = urlService;
    }

    public async Task<ClientShareReportSettingsResponse> ExecuteAsync(ClientShareReportSettingsRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
        RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");

        var client = await _clientDao.GetById(request.ClientId, workspace);
        RecordNotFoundException.ThrowIfNull(client, "Client not found");
        if (!await _securityManager.HasAccess(AccessLevel.Write, user, client))
        {
            throw new HasNoAccessException();
        }

        var report = await _sharedClientReportDao.GetByClientIdAsync(client.Id);
        var isNewReport = report == null;
        report ??= await _sharedClientReportDao.CreateAsync(client, GenerateToken());
        if (request.IsRegenerateToken)
        {
            report.Token = GenerateToken();
        }

        if (isNewReport || request.IsUpdateSettings || request.IsRegenerateToken)
        {
            report.IsActive = request.IsActive;
            report.IsShowTasks = request.IsShowTasks;
            report.UpdatedAt = DateTime.UtcNow;
            await _sharedClientReportDao.SaveAsync(report);
        }

        return new ClientShareReportSettingsResponse
        {
            IsActive = report.IsActive,
            IsShowTasks = report.IsShowTasks,
            Token = report.Token,
            ShareUrl = _urlService.ToFrontendAbsoluteUrl($"/shared/report/client/{report.Token}")
        };
    }

    private static string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
