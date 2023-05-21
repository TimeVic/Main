using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

namespace TimeTracker.Web.Store.Dashboard;

public record struct LoadTasksListAction();

public record struct SetTasksListItemsAction(GetListResponse Response);

public record struct SetTasksListItemAction(TaskDto Task);


public record struct SetIsTasksListLoadingAction(bool IsLoading);
