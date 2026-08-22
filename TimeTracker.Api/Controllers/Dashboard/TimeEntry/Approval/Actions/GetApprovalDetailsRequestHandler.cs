using Api.Requests.Abstractions;
using AutoMapper;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Approval.Actions;

public class GetApprovalDetailsRequestHandler : IAsyncRequestHandler<GetApprovalDetailsRequest, GetApprovalDetailsResponse>
{
    private readonly IMapper _mapper;
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly ISecurityManager _securityManager;
    private readonly ITimeEntryApprovalService _approvalService;
    private readonly IDbSessionProvider _sessionProvider;

    public GetApprovalDetailsRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        ITimeEntryApprovalService approvalService,
        IDbSessionProvider sessionProvider
    )
    {
        _mapper = mapper;
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _securityManager = securityManager;
        _approvalService = approvalService;
        _sessionProvider = sessionProvider;
    }

    public async Task<GetApprovalDetailsResponse> ExecuteAsync(GetApprovalDetailsRequest request)
    {
        var currentManager = await _apiRequestService.GetCurrentUser();
        var workspace = await _userDao.GetUsersWorkspace(currentManager, _apiRequestService.GetCurrentWorkspaceId());
        RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");

        var targetUser = await _userDao.GetById(request.UserId);
        RecordNotFoundException.ThrowIfNull(targetUser, "User not found");

        var entries = await _approvalService.GetDetailsAsync(
            currentManager,
            workspace,
            targetUser,
            request.StartDate,
            request.EndDate
        );

        var projectRates = await _sessionProvider.CurrentSession.Query<WorkspaceMemberProjectAccessEntity>()
            .Where(p => p.WorkspaceMember.Workspace.Id == workspace.Id && p.WorkspaceMember.User.Id == targetUser.Id)
            .ToListAsync();
        var rateMap = projectRates
            .Where(p => p.Project != null)
            .ToDictionary(p => p.Project.Id, p => p.HourlyRate ?? 0);

        var projectGroups = entries
            .GroupBy(e => new
            {
                ProjectId = e.Project?.Id,
                ProjectName = e.Project?.Name ?? "Without project",
                ClientName = e.Project?.Client?.Name
            })
            .Select(pGroup =>
            {
                var devHourlyRate = pGroup.Key.ProjectId.HasValue && rateMap.TryGetValue(pGroup.Key.ProjectId.Value, out var rate)
                    ? rate
                    : 0;

                var taskGroups = pGroup
                    .GroupBy(e => new
                    {
                        TaskId = e.Task?.Id,
                        Title = e.Task?.Title ?? (string.IsNullOrWhiteSpace(e.TaskId) ? "Without task" : e.TaskId),
                        ExternalTaskId = e.Task?.ExternalTaskId ?? e.TaskId
                    })
                    .Select(tGroup =>
                    {
                        var taskEntries = tGroup.OrderBy(e => e.StartTime).ToList();
                        var taskDuration = TimeSpan.FromTicks(taskEntries.Sum(e => e.Duration.Ticks));

                        return new ApprovalTaskDto
                        {
                            TaskId = tGroup.Key.TaskId,
                            Title = tGroup.Key.Title,
                            ExternalTaskId = tGroup.Key.ExternalTaskId,
                            TotalDuration = taskDuration,
                            Entries = _mapper.Map<IReadOnlyList<TimeEntryDto>>(taskEntries)
                        };
                    })
                    .ToList();

                var projectDuration = TimeSpan.FromTicks(pGroup.Sum(e => e.Duration.Ticks));
                var projectDevAmount = pGroup.Sum(e => (decimal)e.Duration.TotalHours * devHourlyRate);
                var projectClientAmount = pGroup.Sum(e => e.IsBillable ? (decimal)e.Duration.TotalHours * (e.HourlyRate ?? 0) : 0);

                return new ApprovalProjectDto
                {
                    ProjectId = pGroup.Key.ProjectId,
                    ProjectName = pGroup.Key.ProjectName,
                    ClientName = pGroup.Key.ClientName,
                    TotalDuration = projectDuration,
                    TotalDeveloperAmount = projectDevAmount,
                    TotalClientAmount = projectClientAmount,
                    Tasks = taskGroups
                };
            })
            .ToList();

        var totalDuration = TimeSpan.FromTicks(entries.Sum(e => e.Duration.Ticks));
        var totalDevAmount = projectGroups.Sum(p => p.TotalDeveloperAmount);
        var totalClientAmount = projectGroups.Sum(p => p.TotalClientAmount);

        return new GetApprovalDetailsResponse
        {
            UserId = targetUser.Id,
            UserName = targetUser.Name,
            PeriodStartDate = request.StartDate,
            PeriodEndDate = request.EndDate,
            TotalDuration = totalDuration,
            TotalDeveloperAmount = totalDevAmount,
            TotalClientAmount = totalClientAmount,
            Projects = projectGroups
        };
    }
}
