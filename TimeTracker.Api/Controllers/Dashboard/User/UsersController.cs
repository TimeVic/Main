using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard.User;

[ApiController]
[Authorize]
[Route("/dashboard/user")]
public class UsersController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost("set-notification-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UpdateTask([FromBody] SetNotificationTokenRequest request)
        => this.RequestAsync(request);
}
