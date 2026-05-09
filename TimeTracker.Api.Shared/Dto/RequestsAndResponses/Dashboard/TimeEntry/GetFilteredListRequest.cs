using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;

public class GetFilteredListRequest: IRequest<GetFilteredListResponse>
{
    [Required]
    [IsPositive]
    public int Page { get; set; }
    public Guid? ClientId { get; set; }
    
    public Guid? ProjectId { get; set; }

    public Guid? MemberId { get; set; }
    
    [StringLength(255)]
    public string? Search { get; set; }
    
    public bool? IsBillable { get; set; }
    
    public DateTime? DateFrom { get; set; }
    
    public DateTime? DateTo { get; set; }
}
