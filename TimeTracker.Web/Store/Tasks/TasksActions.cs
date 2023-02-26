using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

namespace TimeTracker.Web.Store.Tasks;

public record struct LoadListAction();

public record struct SetListItemsAction(GetListResponse Response);

public record struct SetListItemAction(TaskDto Task);

public record struct SetListFilterAction(GetListFilterRequest Filter);

public record struct SetIsListLoading(bool IsLoading);
