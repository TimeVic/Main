using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;

namespace TimeTracker.Web.Store.TasksList;

public record struct LoadListAction(bool IsReload = false, Guid? ProjectId = null);

public record struct SetListItemsAction(GetListResponse Response, Guid? ProjectId = null);

public record struct RemoveListItemsAction(Guid TaskListId);

public record struct SetListItemAction(TaskListDto TaskList);

public record struct SetIsListLoadingAction(bool IsLoading);

public record struct SetSelectedAction(Guid? TaskListId);

public record struct ArchiveTaskListAction(TaskListDto TaskList);
