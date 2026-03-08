using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard.Client;

[ApiController]
[Authorize]
[Route("/dashboard/[controller]")]
public class ClientController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Add([FromBody] AddRequest request)
        => this.RequestAsync()
            .For<ClientDto>()
            .With(request);
    
    [HttpPost("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Add([FromBody] GetListRequest request)
        => this.RequestAsync()
            .For<GetListResponse>()
            .With(request);
}
