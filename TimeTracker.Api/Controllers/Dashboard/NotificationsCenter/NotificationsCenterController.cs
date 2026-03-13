using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard.NotificationsCenter;

[ApiController]
[Authorize]
[Route("/dashboard/notifications-center")]
public class NotificationsCenterController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost("get-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Add([FromBody] GetCountRequest request)
        => this.RequestAsync()
            .For<GetCountResponse>()
            .With(request);
    
    [HttpPost("get-list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetList([FromBody] GetListRequest request)
        => this.RequestAsync()
            .For<GetListResponse>()
            .With(request);
    
    [HttpPost("mark-all-as-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> MarkAllAsRead([FromBody] MarkAllAsReadRequest request)
        => this.RequestAsync(request);
    
    [HttpPost("mark-as-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
        => this.RequestAsync(request);
}
