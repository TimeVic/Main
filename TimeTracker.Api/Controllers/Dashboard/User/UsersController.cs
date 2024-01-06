using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;

namespace TimeTracker.Api.Controllers.Dashboard.User;

[ApiController]
[Authorize]
[Route("/dashboard/user")]
public class UsersController : MainApiControllerBase
{
    public UsersController(
        IAsyncRequestBuilder asyncRequestBuilder, 
        IDbSessionProvider commitPerformer,
        ILogger<UsersController> logger
    ) : base(asyncRequestBuilder, commitPerformer, logger)
    {
    }

    [HttpPost("set-notification-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UpdateTask([FromBody] SetNotificationTokenRequest request)
        => this.RequestAsync(request);
}
