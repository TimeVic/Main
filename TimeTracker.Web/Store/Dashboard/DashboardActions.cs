using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

namespace TimeTracker.Web.Store.Dashboard;

public record struct LoadTasksListAction();

public record struct SetTasksListItemsAction(GetListResponse Response);

public record struct SetIsTasksListLoadingAction(bool IsLoading);
