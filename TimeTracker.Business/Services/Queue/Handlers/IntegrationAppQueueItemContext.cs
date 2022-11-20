using Domain.Abstractions;
using Notification.Abstractions;

namespace TimeTracker.Business.Services.Queue.Handlers;

public class IntegrationAppQueueItemContext: IExternalServiceItemContext
{
    public long TimeEntryId { get; set; }

    public IntegrationAppQueueItemContext()
    {
    }

    public IntegrationAppQueueItemContext(long timeEntryId)
    {
        TimeEntryId = timeEntryId;
    }
}
