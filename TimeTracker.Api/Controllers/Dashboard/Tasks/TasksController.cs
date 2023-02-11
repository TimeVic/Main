using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks;

[ApiController]
[Authorize]
[Route("/dashboard/tasks")]
public class TasksController : MainApiControllerBase
{
    public TasksController(
        IAsyncRequestBuilder asyncRequestBuilder, 
        IDbSessionProvider commitPerformer,
        ILogger<TasksController> logger
    ) : base(asyncRequestBuilder, commitPerformer, logger)
    {
    }

    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> AddTask([FromBody] AddRequest request)
        => this.RequestAsync()
            .For<TaskDto>()
            .With(request);
    
    [HttpPost("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UpdateTask([FromBody] UpdateRequest request)
        => this.RequestAsync()
            .For<TaskDto>()
            .With(request);
    
    [HttpPost("get-list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UpdateTask([FromBody] GetListRequest request)
        => this.RequestAsync()
            .For<GetListResponse>()
            .With(request);
}
