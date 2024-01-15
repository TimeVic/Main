using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

namespace TimeTracker.Web.Store.Tasks;

public record struct LoadListAction();

public record struct LoadOverdueTasksListAction();

public record struct SetListItemsAction(GetListResponse Response);

public record struct SetListItemAction(TaskDto Task);

public record struct SetAttachmentsAction(long TaskId, ICollection<StoredFileDto> Attachments);

public record struct SetListFilterAction(GetListFilterRequest Filter);

public record struct SetIsListLoading(bool IsLoading);

public record struct SetOverdueTasksListItemsAction(GetListResponse Response);

public record struct SetOverdueTasksListItemAction(TaskDto Task);


public record struct SetIsOverdueTasksListLoadingAction(bool IsLoading);

public record struct UpdateTaskAction(
    UpdateRequest UpdateRequest,
    bool IsUpdateState = true
);

public record struct UpdatePositionsAction(
    IEnumerable<TaskDto> Tasks
);

public record struct UpdateListItemsAction(
    IEnumerable<TaskDto> Tasks
);
