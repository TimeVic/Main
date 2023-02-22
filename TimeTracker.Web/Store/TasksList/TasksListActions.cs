using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;

namespace TimeTracker.Web.Store.TasksList;

public record struct LoadListAction(bool IsReload = false);

public record struct SetListItemsAction(GetListResponse Response);

public record struct SetListItemAction(TaskListDto TaskList);

public record struct SetIsListLoading(bool IsLoading);

public record struct SetSelected(long? TaskListId);
