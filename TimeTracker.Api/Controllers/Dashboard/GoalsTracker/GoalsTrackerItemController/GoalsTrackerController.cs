using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard.GoalsTracker.GoalsTrackerItemController;

[ApiController]
[Authorize]
[Route("/dashboard/goals-tracker/item")]
public class GoalsTrackerItemController : MainApiControllerBase
{
    public GoalsTrackerItemController(
        IAsyncRequestBuilder asyncRequestBuilder, 
        IDbSessionProvider commitPerformer,
        ILogger<GoalsTrackerItemController> logger
    ) : base(asyncRequestBuilder, commitPerformer, logger)
    {
    }
    
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Create([FromBody] CreateItemRequest request)
        => this.RequestAsync()
            .For<GoalsTrackerItemDto>()
            .With(request);
    
    [HttpPost("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Create([FromBody] UpdateItemRequest request)
        => this.RequestAsync()
            .For<GoalsTrackerItemDto>()
            .With(request);
    
    [HttpPost("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Create([FromBody] DeleteItemRequest request)
        => this.RequestAsync(request);
    
    [HttpPost("set-completion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> SetCompletion([FromBody] SetCompletionRequest request)
        => this.RequestAsync()
            .For<GoalsTrackerCompletionMarkerDto>()
            .With(request);
}
