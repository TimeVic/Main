using Domain.Abstractions;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Redmine;

namespace TimeTracker.Business.Services.Queue.Handlers;

public class SendDeleteTimeEntryIntegrationRequestHandler : IAsyncQueueHandler<SendDeleteTimeEntryIntegrationRequestContext>
{
    private readonly IClickUpClient _clickUpClient;
    private readonly IRedmineClient _redmineClient;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly ILogger<SendDeleteTimeEntryIntegrationRequestHandler> _logger;

    public SendDeleteTimeEntryIntegrationRequestHandler(
        IClickUpClient clickUpClient,
        IRedmineClient redmineClient,
        IDbSessionProvider sessionProvider,
        ILogger<SendDeleteTimeEntryIntegrationRequestHandler> logger
    )
    {
        _clickUpClient = clickUpClient;
        _redmineClient = redmineClient;
        _sessionProvider = sessionProvider;
        _logger = logger;
    }

    public async Task HandleAsync(SendDeleteTimeEntryIntegrationRequestContext commandContext,
        CancellationToken cancellationToken = default)
    {
        var timeEntriesToDelete = await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(item => item.IsMarkedToDelete == true)
            .ToListAsync(
                cancellationToken: cancellationToken
            );

        foreach (var timeEntry in timeEntriesToDelete)
        {
            var transaction = _sessionProvider.CurrentSession.BeginTransaction();
            try
            {
                try
                {
                    if (string.IsNullOrEmpty(timeEntry.TaskId))
                    {
                        _logger.LogDebug($"TimeEntry does not have TaskId: {timeEntry.TaskId}");
                    }
                    if (
                        timeEntry.Workspace.IsIntegrationClickUpActive(timeEntry.User.Id)
                        && _clickUpClient.IsCorrectTaskId(timeEntry)
                    )
                    {
                        await _clickUpClient.DeleteTimeEntryAsync(timeEntry);
                    }
                    if (
                        timeEntry.Workspace.IsIntegrationRedmineActive(timeEntry.User.Id)
                        && _redmineClient.IsCorrectTaskId(timeEntry)
                    )
                    {
                        await _redmineClient.DeleteTimeEntryAsync(timeEntry);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e, e.Message);
                }

                await _sessionProvider.CurrentSession.DeleteAsync(timeEntry, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (MinorException e)
            {
                _logger.LogDebug(e, e.Message);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw e;
            }
        }
    }
}
