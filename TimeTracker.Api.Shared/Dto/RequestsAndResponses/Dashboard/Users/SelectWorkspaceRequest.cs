using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;

public class SelectWorkspaceRequest : IRequest<UserDto>
{
    [Required]
    public Guid WorkspaceId { get; set; }
}
