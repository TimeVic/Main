using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;

namespace TimeTracker.Web.Store.Payment;

public record struct LoadPaymentListAction(bool IsReload = false);

public record struct SetPaymentListItemsAction(GetListResponse Response);

public record struct SetPaymentListItemAction(PaymentDto Payment);

public record struct SetPaymentIsListLoading(bool IsLoading);

public record struct AddEmptyPaymentListItemAction();

public record struct RemoveEmptyPaymentListItemAction();

public record struct SaveEmptyPaymentListItemAction();

public record struct SavePaymentListItemAction(PaymentDto Payment);
