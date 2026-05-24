using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Client.Core.Store.List.Currency;

public record struct LoadListAction();

public record struct SetListItemsAction(ICollection<CurrencyDto> Items);

public record struct SetIsLoading(bool IsLoading);
