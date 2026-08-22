using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.TimeEntry;

public class TimeEntriesRejectedNotificationItemContext : INotificationItemContext
{
    public Guid UserId { get; set; }
    public Guid WorkspaceId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public ICollection<Guid> TimeEntryIds { get; set; } = new List<Guid>();

    public TimeEntriesRejectedNotificationItemContext()
    {
    }

    public TimeEntriesRejectedNotificationItemContext(
        Guid userId,
        Guid workspaceId,
        string rejectionReason,
        ICollection<Guid> timeEntryIds
    )
    {
        UserId = userId;
        WorkspaceId = workspaceId;
        RejectionReason = rejectionReason;
        TimeEntryIds = timeEntryIds;
    }
}
