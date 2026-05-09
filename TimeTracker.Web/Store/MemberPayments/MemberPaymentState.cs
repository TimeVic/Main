using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.MemberPayments;

[FeatureState]
public record MemberPaymentState
{
    public ICollection<MemberPaymentDto> List { get; set; } = new List<MemberPaymentDto>();

    public int SelectedPage { get; set; } = 1;
    
    public int TotalCount { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasMoreItems { get; set; }
    
    public bool IsListLoading { get; set; }

    public bool IsLoaded { get; set; } = false;
}
