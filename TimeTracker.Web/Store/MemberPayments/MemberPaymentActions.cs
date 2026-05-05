using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;

namespace TimeTracker.Web.Store.MemberPayments;

public record struct LoadMemberPaymentListAction(bool IsReload = false, Guid MemberId = default);

public record struct SetListItemsAction(GetListResponse Response);

public record struct SetListItemAction(MemberPaymentDto MemberPayment);

public record struct UpdateAction(UpdateRequest Request);

public record struct SetIsListLoading(bool IsLoading);

public record struct DeleteMemberPaymentAction(Guid MemberPaymentId);

public record struct AddMemberPaymentAction(AddRequest Request);

public record struct RemoveMemberPaymentListItemAction(Guid MemberPaymentId);
