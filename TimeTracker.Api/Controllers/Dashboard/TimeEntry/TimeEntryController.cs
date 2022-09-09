using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry;

[ApiController]
[Authorize]
[Route("/dashboard/time-entry")]
public class TimeEntryController : MainApiControllerBase
{
    public TimeEntryController(
        IAsyncRequestBuilder asyncRequestBuilder, 
        IDbSessionProvider commitPerformer,
        ILogger<TimeEntryController> logger
    ) : base(asyncRequestBuilder, commitPerformer, logger)
    {
    }

    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Start([FromBody] StartRequest request)
        => this.RequestAsync()
            .For<TimeEntryDto>()
            .With(request);
    
    [HttpPost("stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Stop([FromBody] StopRequest request)
        => this.RequestAsync(request);
    
    [HttpPost("set")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Set([FromBody] SetRequest request)
        => this.RequestAsync()
            .For<TimeEntryDto>()
            .With(request);
    
    [HttpPost("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Set([FromBody] DeleteRequest request)
        => this.RequestAsync(request);
    
    [HttpPost("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> List([FromBody] GetListRequest request)
        => this.RequestAsync()
            .For<GetListResponse>()
            .With(request);
}
