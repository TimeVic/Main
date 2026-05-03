using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.ClientPayments;

[FeatureState]
public record ClientPaymentState
{
    public ICollection<ClientPaymentDto> List { get; set; } = new List<ClientPaymentDto>();

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public bool HasMoreItems { get; set; }

    public bool IsListLoading { get; set; }

    public bool IsLoaded { get; set; }
}
