using Domain.Abstractions;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;

namespace TimeTracker.Business.Services.Queue.Handlers;

public class IntegrationAppQueueHandler: IAsyncQueueHandler<IntegrationAppQueueItemContext>, IDisposable
{
    private readonly IClickUpClient _clickUpClient;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly ILogger<IntegrationAppQueueHandler> _logger;
    private readonly ISession _session;

    public IntegrationAppQueueHandler(
        IClickUpClient clickUpClient,
        IDbSessionProvider sessionProvider,
        ILogger<IntegrationAppQueueHandler> logger
    )
    {
        _clickUpClient = clickUpClient;
        _sessionProvider = sessionProvider;
        _logger = logger;
        _session = _sessionProvider.CreateSession();
    }
    
    public async Task HandleAsync(IntegrationAppQueueItemContext commandContext, CancellationToken cancellationToken = default)
    {
        var transaction = _session.BeginTransaction();
        try
        {
            var timeEntry = await _session.Query<TimeEntryEntity>().FirstOrDefaultAsync(
                item => item.Id == commandContext.TimeEntryId,
                cancellationToken: cancellationToken
            );
            if (timeEntry == null)
            {
                throw new MinorException($"TimeEntry not found: {commandContext.TimeEntryId}");
            }

            if (string.IsNullOrEmpty(timeEntry.TaskId))
            {
                throw new MinorException($"TimeEntry does not have TaskId: {timeEntry.TaskId}");
            }

            if (timeEntry.Workspace.IsIntegrationClickUpActive(timeEntry.User.Id))
            {
                var setResponse = await _clickUpClient.SendTimeEntryAsync(timeEntry);
                if (setResponse is {IsError: false})
                {
                    timeEntry.ClickUpId = setResponse.Value.Id;
                    await _session.SaveAsync(timeEntry, cancellationToken);
                }
                var getTaskResponse = await _clickUpClient.GetTaskAsync(timeEntry);
                if (getTaskResponse != null)
                {
                    timeEntry.Description = getTaskResponse.Value.Name;
                    await _session.SaveAsync(timeEntry, cancellationToken);
                }
            }

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

    public void Dispose()
    {
        _session.Dispose();
    }
}
