using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Massaging.Channel;

[ApiController]
[Authorize]
[Route("/messaging/[controller]")]
public class ChannelController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Create([FromBody] CreateRequest request)
        => this.RequestAsync(request);
}
