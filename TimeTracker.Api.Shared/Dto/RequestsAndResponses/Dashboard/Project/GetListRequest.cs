using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;

public class GetListRequest: IRequest<GetListResponse>
{
    [Required]
    [IsPositive]
    public int Page { get; set; }
    
    [Required]
    [IsPositive]
    public long WorkspaceId { get; set; }
}
