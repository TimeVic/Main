using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;

namespace TimeTracker.Web.Store.NotificationCenter;

public record struct LoadUnreadCountAction();

public record struct LoadListAction(bool IsRefresh = true);

public record struct MarkAllAsReadAction();

public record struct MarkAsReadAction(long NotificationId);

public record struct SetAllAsReadAction();

public record struct SetAsReadAction(long NotificationId);

public record struct RefreshListAction();

public record struct SetListAction(GetListResponse Response);

public record struct SetUnreadCountAction(int Count);

public record struct SetIsListLoadingAction(bool IsLoading);
