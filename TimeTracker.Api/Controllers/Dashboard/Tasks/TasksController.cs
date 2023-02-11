using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;

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

    #region Task List
    [HttpPost("list/add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> AddTaskList([FromBody] AddTaskListRequest request)
        => this.RequestAsync()
            .For<TaskListDto>()
            .With(request);
    
    [HttpPost("list/update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UpdateTaskList([FromBody] UpdateTaskListRequest request)
        => this.RequestAsync()
            .For<TaskListDto>()
            .With(request);
    
    [HttpPost("list/get-list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UpdateTaskList([FromBody] GetTaskListRequest request)
        => this.RequestAsync()
            .For<GetTaskListResponse>()
            .With(request);
    
    #endregion
    
    #region Task
    
    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> AddTask([FromBody] AddTaskRequest request)
        => this.RequestAsync()
            .For<TaskDto>()
            .With(request);
    
    [HttpPost("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UpdateTask([FromBody] UpdateTaskRequest request)
        => this.RequestAsync()
            .For<TaskDto>()
            .With(request);
    
    #endregion
}
