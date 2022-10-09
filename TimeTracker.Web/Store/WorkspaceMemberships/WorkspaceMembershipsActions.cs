using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;

namespace TimeTracker.Web.Store.WorkspaceMemberships;

public record struct LoadListAction(bool IsReload = false);

public record struct SetListItemsAction(GetListResponse Response);

public record struct SetListItemAction(ClientDto Client);

public record struct SetIsListLoading(bool IsLoading);

public record struct AddNewMemberAction(string Email);
