using Domain.Abstractions;
using Notification.Abstractions;

namespace TimeTracker.Business.Services.Queue.Handlers;

public class SendSetTimeEntryIntegrationRequestContext: IExternalServiceItemContext
{
    public long TimeEntryId { get; set; }

    public SendSetTimeEntryIntegrationRequestContext()
    {
    }

    public SendSetTimeEntryIntegrationRequestContext(long timeEntryId)
    {
        TimeEntryId = timeEntryId;
    }
}
