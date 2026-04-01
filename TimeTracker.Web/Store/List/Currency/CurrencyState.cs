using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.List;

namespace TimeTracker.Web.Store.List.Currency;

[FeatureState]
public record CurrencyState
{
    public ICollection<CurrencyDto> List { get; set; } = new List<CurrencyDto>();
    public bool IsLoading { get; set; } = false;
}
