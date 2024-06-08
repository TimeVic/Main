using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;

namespace TimeTracker.Web.Store.Tag;

public record struct LoadListAction(bool IsReload = false);

public record struct SetListItemsAction(GetListResponse Response);

public record struct DeleteItemAction(TagDto Tag);

public record struct SetListItemAction(TagDto Tag);

public record struct DeleteListItemAction(long TagId);

public record struct SetIsListLoading(bool IsLoading);
