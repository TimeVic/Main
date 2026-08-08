using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Core.Store.Tasks;

public record struct LoadListAction(Guid? TaskListId, GetListFilterRequest? Filter = null);

public record struct LoadOverdueTasksListAction();

public record struct SetListItemsAction(GetListResponse Response);

public record struct SetListItemAction(TaskDto Task);

public record struct RemoveListItemAction(Guid TaskId);

public record struct SetListFilterAction(GetListFilterRequest Filter);

public record struct SetIsListLoading(bool IsLoading);

public record struct SetIsTaskSavingAction(bool IsSaving);

public record struct ToggleStatusExpansionAction(TaskStatus Status);

public record struct SetOverdueTasksListItemsAction(GetListResponse Response);

public record struct SetOverdueTasksListItemAction(TaskDto Task);


public record struct SetIsOverdueTasksListLoadingAction(bool IsLoading);

public record struct UpdateTaskAction(
    UpdateRequest UpdateRequest,
    bool IsUpdateState = true,
    bool IsShowToast = false
);

public record struct UpdatePositionsAction(
    IEnumerable<TaskDto> Tasks
);

public record struct UpdateListItemsAction(
    IEnumerable<TaskDto> Tasks
);
