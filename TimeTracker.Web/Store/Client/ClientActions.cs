using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;

namespace TimeTracker.Web.Store.Client;

public record struct LoadListAction(bool IsReload = false);

public record struct SetClientListItemsAction(GetListResponse Response);

public record struct SetClientListItemAction(ClientDto Client);

public record struct SetClientIsListLoading(bool IsLoading);

public record struct AddEmptyClientListItemAction();

public record struct RemoveEmptyClientListItemAction();

public record struct SaveEmptyClientListItemAction();
