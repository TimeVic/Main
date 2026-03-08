using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks;

[ApiController]
[Authorize]
[Route("/dashboard/tasks")]
public class TasksController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
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
    
    [HttpPost("update-positions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UpdateTask([FromBody] UpdatePositionsRequest request)
        => this.RequestAsync(request);
    
    [HttpPost("get-list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetList([FromBody] GetListRequest request)
        => this.RequestAsync()
            .For<GetListResponse>()
            .With(request);
    
    [HttpPost("get-my-list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetMyList([FromBody] GetMyListRequest request)
        => this.RequestAsync()
            .For<GetListResponse>()
            .With(request);
    
    [HttpPost("get-for-calendar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetForCalendar([FromBody] GetForCalendarRequest request)
        => this.RequestAsync()
            .For<GetListResponse>()
            .With(request);
    
    [HttpPost("get-one")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetOneTask([FromBody] GetOneRequest request)
        => this.RequestAsync()
            .For<TaskDto>()
            .With(request);
}
