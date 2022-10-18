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
    
    [Required]
    [IsPositive]
    public long WorkspaceId { get; set; }
    
    [IsPositive]
    public long? ClientId { get; set; }
    
    [IsPositive]
    public long? ProjectId { get; set; }

    [IsPositive]
    public long? MemberId { get; set; }
    
    [StringLength(255)]
    public string? Search { get; set; }
    
    public bool? IsBillable { get; set; }
}
