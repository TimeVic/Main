using Domain.Abstractions;

namespace TimeTracker.Business.Services.Queue.Handlers;

public class IntegrationAppQueueItemContext: IQueueItemContext
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
