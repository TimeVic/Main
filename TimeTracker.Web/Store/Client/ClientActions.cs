using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;

namespace TimeTracker.Web.Store.Client;

public record struct LoadListAction(bool IsReload = false);

public record struct AddAction(AddRequest Request);

public record struct SetListItemsAction(GetListResponse Response);

public record struct SetListItemAction(ClientDto Client);

public record struct UpdateAction(ClientDto Client);

public record struct SetIsListLoading(bool IsLoading);

public record struct SetIsSavingAction(bool IsSaving);
