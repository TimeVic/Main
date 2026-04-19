using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Security;

public class GetWorkspacePermissionsRequest : IRequest<GetWorkspacePermissionsResponse>
{
    [Required]
    [RequiredNonEmpty]
    public Guid WorkspaceId { get; set; }
}
