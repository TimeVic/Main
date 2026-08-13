using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;

public class SetModeRequest : IRequest<WorkspaceDto>
{
    [Required]
    public WorkspaceMode Mode { get; set; }
}
