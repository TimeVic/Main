using Domain.Abstractions;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Jira;
using TimeTracker.Business.Services.ExternalClients.Redmine;

namespace TimeTracker.Business.Services.Queue.Handlers;

public class SendSetTimeEntryIntegrationRequestHandler: IAsyncQueueHandler<SendSetTimeEntryIntegrationRequestContext>
{
    private readonly IClickUpClient _clickUpClient;
    private readonly IRedmineClient _redmineClient;
    private readonly IJiraClient _jiraClient;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly ILogger<SendSetTimeEntryIntegrationRequestHandler> _logger;

    public SendSetTimeEntryIntegrationRequestHandler(
        IClickUpClient clickUpClient,
        IRedmineClient redmineClient,
        IJiraClient jiraClient,
        IDbSessionProvider sessionProvider,
        ILogger<SendSetTimeEntryIntegrationRequestHandler> logger
    )
    {
        _clickUpClient = clickUpClient;
        _redmineClient = redmineClient;
        _jiraClient = jiraClient;
        _sessionProvider = sessionProvider;
        _logger = logger;
    }
    
    public async Task HandleAsync(SendSetTimeEntryIntegrationRequestContext commandContext, CancellationToken cancellationToken = default)
    {
        var transaction = _sessionProvider.CurrentSession.BeginTransaction();
        try
        {
            var timeEntry = await _sessionProvider.CurrentSession.Query<TimeEntryEntity>().FirstOrDefaultAsync(
                item => item.Id == commandContext.TimeEntryId,
                cancellationToken: cancellationToken
            );
            if (timeEntry == null)
            {
                throw new MinorException($"TimeEntry not found: {commandContext.TimeEntryId}");
            }

            if (string.IsNullOrEmpty(timeEntry.ExternalTaskId))
            {
                throw new MinorException($"TimeEntry does not have TaskId: {commandContext.TimeEntryId}");
            }

            bool isSynchronized = false;
            if (timeEntry.Workspace.IsIntegrationClickUpActive(timeEntry.User.Id))
            {
                var setResponse = await _clickUpClient.SetTimeEntryAsync(timeEntry);
                if (setResponse is {IsError: false})
                {
                    timeEntry.ClickUpId = setResponse.Id;
                    if (await _clickUpClient.IsFillTimeEntryDescriptionFromTaskTitle(timeEntry))
                    {
                        timeEntry.Description = setResponse.AdditionalDescription;
                        isSynchronized = true;
                    }
                    await _sessionProvider.CurrentSession.SaveAsync(timeEntry, cancellationToken);
                }
            }
            if (timeEntry.Workspace.IsIntegrationRedmineActive(timeEntry.User.Id))
            {
                var setResponse = await _redmineClient.SetTimeEntryAsync(timeEntry);
                if (setResponse is {IsError: false})
                {
                    timeEntry.RedmineId = setResponse.Id;
                    if (
                        await _redmineClient.IsFillTimeEntryDescriptionFromTaskTitle(timeEntry)
                        && !isSynchronized
                    )
                    {
                        timeEntry.Description = setResponse.AdditionalDescription;
                    }
                    await _sessionProvider.CurrentSession.SaveAsync(timeEntry, cancellationToken);
                }
            }
            if (timeEntry.Workspace.IsIntegrationJiraActive(timeEntry.User.Id))
            {
                var setResponse = await _jiraClient.SetTimeEntryAsync(timeEntry);
                if (setResponse is {IsError: false})
                {
                    timeEntry.JiraId = long.Parse(setResponse.Id);
                    if (
                        await _jiraClient.IsFillTimeEntryDescriptionFromTaskTitle(timeEntry)
                        && !isSynchronized
                    )
                    {
                        timeEntry.Description = setResponse.AdditionalDescription;
                    }
                    await _sessionProvider.CurrentSession.SaveAsync(timeEntry, cancellationToken);
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (MinorException e)
        {
            _logger.LogDebug(e.Message);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw e;
        }
    }
}
