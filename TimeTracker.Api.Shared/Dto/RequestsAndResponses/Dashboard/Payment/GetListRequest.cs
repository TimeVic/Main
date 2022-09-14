using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;

public class GetListRequest: IRequest<GetListResponse>
{
    [Required]
    [IsPositive]
    public long WorkspaceId { get; set; }
}
