using Domain.Abstractions;
using Notification.Abstractions;

namespace TimeTracker.Business.Services.Queue.Handlers;

public class SendSetTimeEntryIntegrationRequestContext: IExternalServiceItemContext
{
    public Guid TimeEntryId { get; set; }

    public SendSetTimeEntryIntegrationRequestContext()
    {
    }

    public SendSetTimeEntryIntegrationRequestContext(Guid timeEntryId)
    {
        TimeEntryId = timeEntryId;
    }
}
