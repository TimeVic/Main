using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;

namespace TimeTracker.Web.Store.ClientPayments;

public record struct LoadClientPaymentListAction(bool IsReload = false);

public record struct SetClientPaymentListItemsAction(GetListResponse Response);

public record struct SetClientPaymentListItemAction(ClientPaymentDto ClientPayment);

public record struct UpdateClientPaymentAction(UpdateRequest Request);

public record struct SetClientPaymentIsListLoadingAction(bool IsLoading);

public record struct DeleteClientPaymentAction(Guid ClientPaymentId);

public record struct AddClientPaymentAction(AddRequest Request);

public record struct RemoveClientPaymentListItemAction(Guid ClientPaymentId);
