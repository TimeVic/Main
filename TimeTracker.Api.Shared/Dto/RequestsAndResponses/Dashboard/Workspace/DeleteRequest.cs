using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;

public class DeleteRequest : IRequest
{
    [Required]
    public Guid WorkspaceId { get; set; }

    [Required]
    [StringLength(256)]
    public string ConfirmationName { get; set; } = string.Empty;
}
