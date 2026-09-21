using System.ComponentModel;
using AutoMapper;
using ModelContextProtocol.Server;
using NHibernate;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Mcp.Context;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Mcp.Tools;

[McpServerToolType]
public sealed class TimeEntryTools
{
    private readonly IMcpContextService _mcpContext;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITimeEntryService _timeEntryService;
    private readonly IProjectDao _projectDao;
    private readonly IProjectService _projectService;
    private readonly ISecurityManager _securityManager;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IMapper _mapper;

    public TimeEntryTools(
        IMcpContextService mcpContext,
        ITimeEntryDao timeEntryDao,
        ITimeEntryService timeEntryService,
        IProjectDao projectDao,
        IProjectService projectService,
        ISecurityManager securityManager,
        IWorkspaceAccessService workspaceAccessService,
        IDbSessionProvider sessionProvider,
        IMapper mapper
    )
    {
        _mcpContext = mcpContext;
        _timeEntryDao = timeEntryDao;
        _timeEntryService = timeEntryService;
        _projectDao = projectDao;
        _projectService = projectService;
        _securityManager = securityManager;
        _workspaceAccessService = workspaceAccessService;
        _sessionProvider = sessionProvider;
        _mapper = mapper;
    }

    [McpServerTool(Name = "time_entry_set")]
    [Description("Creates or updates a time entry in the specified workspace with start time and optional end time.")]
    public async Task<TimeEntryDto> Set(
        [Description("Workspace ID to create or update time entry in")] Guid workspaceId,
        [Description("Start date and time in UTC")] DateTime startTime,
        [Description("End date and time in UTC (optional)")] DateTime? endTime = null,
        [Description("Existing time entry ID to update, or omit to create a new entry")] Guid? id = null,
        [Description("Project ID to associate with the time entry")] Guid? projectId = null,
        [Description("Description of the work done")] string? description = null,
        [Description("Hourly billing rate")] decimal? hourlyRate = null,
        [Description("Whether this time entry is billable")] bool isBillable = false
    )
    {
        var (user, workspace) = await _mcpContext.GetUserAndWorkspaceAsync(workspaceId);
        if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
        {
            throw new HasNoAccessException();
        }

        var timeEntry = id.HasValue ? await _timeEntryDao.GetByIdAsync(id.Value) : null;
        if (timeEntry != null && !await _securityManager.HasAccess(AccessLevel.Write, user, timeEntry))
        {
            throw new HasNoAccessException();
        }

        var userAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
        var userProjects = await _projectDao.GetAvailableForUserListAsync(workspace, user, userAccess);
        var project = projectId.HasValue ? userProjects.Items.FirstOrDefault(item => item.Id == projectId.Value) : null;
        if (isBillable && !hourlyRate.HasValue)
        {
            hourlyRate = await _projectService.GetUsersHourlyRateForProject(user, project);
        }

        timeEntry = await _timeEntryService.SetAsync(
            user,
            workspace,
            new TimeEntryCreationDto
            {
                Id = timeEntry?.Id,
                Description = description,
                StartTime = startTime,
                EndTime = endTime,
                HourlyRate = hourlyRate,
                IsBillable = isBillable
            },
            project
        );

        await _sessionProvider.CurrentSession.FlushAsync();
        return _mapper.Map<TimeEntryDto>(timeEntry);
    }
}

