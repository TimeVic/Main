using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;

namespace TimeTracker.Web.Store.GoalsTracker;

public record struct LoadTrackerAction(DateTime Date);

public record struct ChangePositionsAction(ICollection<GoalsTrackerItemDto> Goals);

public record struct CreateTrackerItemAction(string Name, int NumberOfTimes);

public record struct UpdateTrackerItemAction(UpdateItemRequest Request);

public record struct DeleteTrackerItemAction(GoalsTrackerItemDto Item);

public record struct DeleteTrackerItemFromListAction(GoalsTrackerItemDto Item);

public record struct SetItemCompletionAction(GoalsTrackerItemDto Item, int DayOfMonth, bool IsChecked);

public record struct SetCompletionItemsAction(GoalsTrackerItemDto Item, ICollection<GoalsTrackerCompletionMarkerDto> CompletionMarkers);

public record struct SetCompletionItemAction(GoalsTrackerItemDto Item, GoalsTrackerCompletionMarkerDto CompletionMarker);

public record struct SetIsListLoadingAction(bool IsLoading);

public record struct SetTrackerAction(GoalsTrackerDto Tracker);

public record struct SetGoalsTrackerItemAction(GoalsTrackerItemDto Item);

public record struct SetGoalsTrackerItemsAction(ICollection<GoalsTrackerItemDto> Items);
