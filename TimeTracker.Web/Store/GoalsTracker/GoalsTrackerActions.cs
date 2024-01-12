using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;

namespace TimeTracker.Web.Store.GoalsTracker;

public record struct LoadTrackerAction(int Year, int Month);

public record struct CheckGoalItemAction(GoalsTrackerItemDto Item, int DayOfMonth, bool IsChecked);

public record struct SetCompletionItemsAction(GoalsTrackerItemDto Item, ICollection<GoalsTrackerCompletionMarkerDto> CompletionMarkers);

public record struct SetIsListLoadingAction(bool IsLoading);

public record struct SetTrackerAction(GoalsTrackerDto Tracker);
