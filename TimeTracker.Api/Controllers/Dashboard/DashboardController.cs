using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Counters;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Init;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard;

[ApiController]
[Authorize]
[Route("/dashboard")]
public class DashboardController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost("init")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Init([FromBody] DashboardInitRequest request)
        => this.RequestAsync()
            .For<DashboardInitResponse>()
            .With(request);

    [HttpPost("counters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetCounters([FromBody] GetCountersRequest request)
        => this.RequestAsync()
            .For<GetCountersResponse>()
            .With(request);
}
