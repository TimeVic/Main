using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Web.Store.WorkspaceMemberships;

public record struct LoadListAction(bool IsReload = false);

public record struct SetListItemsAction(GetListResponse Response);

public record struct SetListItemAction(ClientDto Client);

public record struct SetIsListLoading(bool IsLoading);

public record struct AddNewMemberAction(string Email);

public record struct UpdateMemberAction(
    long MembershipId,
    MembershipAccessType Access,
    ICollection<ProjectDto>? Projects
);

public record struct DeleteMemberAction(WorkspaceMembershipDto Membership);
