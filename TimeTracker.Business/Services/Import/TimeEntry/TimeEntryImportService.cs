using Microsoft.Extensions.Logging;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants.Import;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Import.TimeEntry.Model;

namespace TimeTracker.Business.Services.Import.TimeEntry;

public class TimeEntryImportService : ITimeEntryImportService
{
    private const string DefaultProjectName = "Default";

    private readonly ITimeEntryCsvParserFactory _parserFactory;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IClientDao _clientDao;
    private readonly IProjectDao _projectDao;
    private readonly ITagDao _tagDao;
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly ILogger<TimeEntryImportService> _logger;

    public TimeEntryImportService(
        ITimeEntryCsvParserFactory parserFactory,
        ITimeEntryDao timeEntryDao,
        IClientDao clientDao,
        IProjectDao projectDao,
        ITagDao tagDao,
        IDbSessionProvider dbSessionProvider,
        ILogger<TimeEntryImportService> logger
    )
    {
        _parserFactory = parserFactory;
        _timeEntryDao = timeEntryDao;
        _clientDao = clientDao;
        _projectDao = projectDao;
        _tagDao = tagDao;
        _dbSessionProvider = dbSessionProvider;
        _logger = logger;
    }

    public async Task<TimeEntryImportResultDto> ImportAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        Stream fileStream,
        TimeEntryImportSourceType sourceType,
        bool defaultIsBillable,
        decimal? defaultHourlyRate,
        CancellationToken cancellationToken = default
    )
    {
        var parser = _parserFactory.GetParser(sourceType);
        var parsedModels = await parser.ParseAsync(fileStream, cancellationToken);

        if (parsedModels.Count == 0)
        {
            return new TimeEntryImportResultDto
            {
                ImportedCount = 0,
                SkippedCount = 0,
                TotalCount = 0
            };
        }

        var timeZoneInfo = ResolveTimeZone(workspace.TimeZone);

        // Load existing clients and their projects for the workspace
        var existingClients = workspace.Clients.ToList();

        var clientsByName = new Dictionary<string, ClientEntity>(StringComparer.OrdinalIgnoreCase);
        var projectsByClientAndName = new Dictionary<string, ProjectEntity>(StringComparer.OrdinalIgnoreCase);

        foreach (var client in existingClients)
        {
            clientsByName[client.Name.Trim()] = client;
            foreach (var project in client.Projects.Where(p => !p.IsArchived))
            {
                projectsByClientAndName[$"{client.Id}_{project.Name.Trim()}"] = project;
            }
        }

        // Load existing workspace tags
        var existingTags = await _tagDao.GetList(workspace);
        var tagsByName = new Dictionary<string, TagEntity>(StringComparer.OrdinalIgnoreCase);
        foreach (var tag in existingTags)
        {
            tagsByName[tag.Name.Trim()] = tag;
        }

        // Preload existing time entries for uniqueness check
        var minDate = ConvertToUtc(parsedModels.Min(m => m.StartTime).AddDays(-2), timeZoneInfo);
        var maxDate = ConvertToUtc(parsedModels.Max(m => m.EndTime ?? m.StartTime).AddDays(2), timeZoneInfo);

        var existingEntries = await _timeEntryDao.GetListInRangeAsync(
            workspace,
            user,
            minDate,
            maxDate,
            cancellationToken
        );

        var existingKeys = new HashSet<string>();
        foreach (var e in existingEntries)
        {
            existingKeys.Add(BuildUniquenessKey(e.StartTime, e.EndTime, e.Project?.Id));
        }

        var sourceClientName = sourceType == TimeEntryImportSourceType.Clockify ? "Clockify" : sourceType.ToString();
        var importedCount = 0;
        var skippedCount = 0;

        foreach (var model in parsedModels)
        {
            var startTimeUtc = ConvertToUtc(model.StartTime, timeZoneInfo);
            DateTime? endTimeUtc = model.EndTime.HasValue
                ? ConvertToUtc(model.EndTime.Value, timeZoneInfo)
                : null;

            var clientName = model.ClientName?.Trim();
            var projectName = model.ProjectName?.Trim();

            ProjectEntity? project = null;

            if (string.IsNullOrEmpty(clientName) && string.IsNullOrEmpty(projectName))
            {
                // No client and no project -> import without project
                project = null;
            }
            else if (!string.IsNullOrEmpty(clientName) && string.IsNullOrEmpty(projectName))
            {
                // Has client, no project -> create/get default project for client
                var client = await GetOrCreateClientAsync(clientName, workspace, clientsByName);
                project = await GetOrCreateProjectAsync(DefaultProjectName, client, projectsByClientAndName);
            }
            else if (string.IsNullOrEmpty(clientName) && !string.IsNullOrEmpty(projectName))
            {
                // Has project, no client -> create/get client with source name (e.g. Clockify)
                var client = await GetOrCreateClientAsync(sourceClientName, workspace, clientsByName);
                project = await GetOrCreateProjectAsync(projectName, client, projectsByClientAndName);
            }
            else
            {
                // Both client and project are present
                var client = await GetOrCreateClientAsync(clientName!, workspace, clientsByName);
                project = await GetOrCreateProjectAsync(projectName!, client, projectsByClientAndName);
            }

            // Uniqueness check: (StartTimeUtc, EndTimeUtc, ProjectId)
            var uniquenessKey = BuildUniquenessKey(startTimeUtc, endTimeUtc, project?.Id);
            if (existingKeys.Contains(uniquenessKey))
            {
                skippedCount++;
                continue;
            }
            existingKeys.Add(uniquenessKey);

            // Tags resolution
            var entryTags = new List<TagEntity>();
            foreach (var tagName in model.Tags)
            {
                var cleanTag = tagName.Trim();
                if (string.IsNullOrEmpty(cleanTag))
                {
                    continue;
                }

                if (!tagsByName.TryGetValue(cleanTag, out var tagEntity))
                {
                    tagEntity = await _tagDao.CreateAsync(workspace, cleanTag);
                    tagsByName[cleanTag] = tagEntity;
                }
                entryTags.Add(tagEntity);
            }

            // Billable & Hourly Rate resolution
            var isBillable = model.IsBillable ?? defaultIsBillable;
            decimal? hourlyRate = model.HourlyRate;
            if (hourlyRate == null && isBillable)
            {
                hourlyRate = defaultHourlyRate ?? project?.DefaultHourlyRate;
            }
            else if (!isBillable)
            {
                hourlyRate = null;
            }

            var timeEntry = new TimeEntryEntity
            {
                Workspace = workspace,
                User = user,
                Project = project,
                Description = model.Description,
                TaskId = model.TaskId,
                StartTime = startTimeUtc,
                EndTime = endTimeUtc,
                IsBillable = isBillable,
                HourlyRate = hourlyRate,
                TimeZone = workspace.TimeZone ?? "UTC",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Tags = entryTags
            };

            await _dbSessionProvider.CurrentSession.SaveAsync(timeEntry, cancellationToken);
            importedCount++;
        }

        await _dbSessionProvider.CurrentSession.FlushAsync(cancellationToken);

        _logger.LogInformation(
            "Import completed for workspace {WorkspaceId} by user {UserId}. Imported: {ImportedCount}, Skipped: {SkippedCount}, Total: {TotalCount}",
            workspace.Id,
            user.Id,
            importedCount,
            skippedCount,
            parsedModels.Count
        );

        return new TimeEntryImportResultDto
        {
            ImportedCount = importedCount,
            SkippedCount = skippedCount,
            TotalCount = parsedModels.Count
        };
    }

    private async Task<ClientEntity> GetOrCreateClientAsync(
        string name,
        WorkspaceEntity workspace,
        Dictionary<string, ClientEntity> clientsByName
    )
    {
        var cleanName = name.Trim();
        if (clientsByName.TryGetValue(cleanName, out var existingClient))
        {
            return existingClient;
        }

        var client = await _clientDao.CreateAsync(workspace, cleanName);
        clientsByName[cleanName] = client;
        return client;
    }

    private async Task<ProjectEntity> GetOrCreateProjectAsync(
        string name,
        ClientEntity client,
        Dictionary<string, ProjectEntity> projectsByClientAndName
    )
    {
        var cleanName = name.Trim();
        var key = $"{client.Id}_{cleanName}";
        if (projectsByClientAndName.TryGetValue(key, out var existingProject))
        {
            return existingProject;
        }

        var project = await _projectDao.CreateAsync(client, cleanName);
        projectsByClientAndName[key] = project;
        return project;
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }

    private static DateTime ConvertToUtc(DateTime localDateTime, TimeZoneInfo timeZoneInfo)
    {
        try
        {
            return TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified),
                timeZoneInfo
            );
        }
        catch
        {
            return DateTime.SpecifyKind(localDateTime, DateTimeKind.Utc);
        }
    }

    private static string BuildUniquenessKey(DateTime startUtc, DateTime? endUtc, Guid? projectId)
    {
        var startFormatted = startUtc.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var endFormatted = endUtc.HasValue ? endUtc.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : "none";
        var projectFormatted = projectId.HasValue ? projectId.Value.ToString() : "none";
        return $"{startFormatted}_{endFormatted}_{projectFormatted}";
    }
}
