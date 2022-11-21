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

public class SendDeleteTimeEntryIntegrationRequestHandler 
    : IAsyncQueueHandler<SendDeleteTimeEntryIntegrationRequestContext>, IDisposable
{
    private readonly IClickUpClient _clickUpClient;
    private readonly IRedmineClient _redmineClient;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly ILogger<SendDeleteTimeEntryIntegrationRequestHandler> _logger;
    private readonly ISession _session;

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
        _session = _sessionProvider.CreateSession();
    }

    public async Task HandleAsync(SendDeleteTimeEntryIntegrationRequestContext commandContext,
        CancellationToken cancellationToken = default)
    {
        var timeEntriesToDelete = await _session.Query<TimeEntryEntity>()
            .Where(item => item.IsMarkedToDelete == true)
            .ToListAsync(
                cancellationToken: cancellationToken
            );

        foreach (var timeEntry in timeEntriesToDelete)
        {
            var transaction = _session.BeginTransaction();
            try
            {
                try
                {
                    if (string.IsNullOrEmpty(timeEntry.TaskId))
                    {
                        _logger.LogTrace($"TimeEntry does not have TaskId: {timeEntry.TaskId}");
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
                    _logger.LogTrace(e, e.Message);
                }

                await _session.DeleteAsync(timeEntry, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (MinorException e)
            {
                _logger.LogTrace(e, e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                await transaction.RollbackAsync(cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        _session.Dispose();
    }
}
