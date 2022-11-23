using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace;

[ApiController]
[Authorize]
[Route("/dashboard/[controller]")]
public class WorkspaceController : MainApiControllerBase
{
    public WorkspaceController(
        IAsyncRequestBuilder asyncRequestBuilder, 
        IDbSessionProvider commitPerformer,
        ILogger<WorkspaceController> logger
    ) : base(asyncRequestBuilder, commitPerformer, logger)
    {
    }

    [HttpPost("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> List([FromBody] GetListRequest request)
        => this.RequestAsync()
            .For<PaginatedListDto<WorkspaceDto>>()
            .With(request);
    
    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Add([FromBody] AddRequest request)
        => this.RequestAsync()
            .For<WorkspaceDto>()
            .With(request);
    
    [HttpPost("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Add([FromBody] UpdateRequest request)
        => this.RequestAsync()
            .For<WorkspaceDto>()
            .With(request);
    
    [HttpPost("settings/integrations/get")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> SetSettings([FromBody] GetIntegrationSettingsRequest request)
        => this.RequestAsync()
            .For<GetIntegrationSettingsResponse>()
            .With(request);
    
    [HttpPost("settings/set-redmine")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> SetRedmineSettings([FromBody] SetRedmineSettingsRequest request)
        => this.RequestAsync()
            .For<WorkspaceSettingsRedmineDto>()
            .With(request);
    
    [HttpPost("settings/set-clickup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> SetRedmineSettings([FromBody] SetClickUpSettingsRequest request)
        => this.RequestAsync()
            .For<WorkspaceSettingsClickUpDto>()
            .With(request);
}
