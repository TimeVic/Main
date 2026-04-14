using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;

namespace TimeTracker.Web.Store.Payment;

public record struct LoadPaymentListAction(bool IsReload = false);

public record struct SetListItemsAction(GetListResponse Response);

public record struct SetListItemAction(PaymentDto Payment);

public record struct UpdateAction(UpdateRequest Request);

public record struct SetIsListLoading(bool IsLoading);

public record struct DeletePaymentAction(Guid PaymentId);

public record struct AddPaymentAction(AddRequest Request);

public record struct RemovePaymentListItemAction(Guid PaymentId);
