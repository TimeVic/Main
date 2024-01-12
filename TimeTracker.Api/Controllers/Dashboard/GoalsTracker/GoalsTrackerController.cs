using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;

namespace TimeTracker.Api.Controllers.Dashboard.GoalsTracker;

[ApiController]
[Authorize]
[Route("/dashboard/goals-tracker")]
public class GoalsTrackerController : MainApiControllerBase
{
    public GoalsTrackerController(
        IAsyncRequestBuilder asyncRequestBuilder, 
        IDbSessionProvider commitPerformer,
        ILogger<GoalsTrackerController> logger
    ) : base(asyncRequestBuilder, commitPerformer, logger)
    {
    }

    [HttpPost("add-goal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Add([FromBody] AddRequest request)
        => this.RequestAsync()
            .For<ClientDto>()
            .With(request);
}
