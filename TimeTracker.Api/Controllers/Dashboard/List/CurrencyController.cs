using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.List.Currency;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard.List;

[ApiController]
[Authorize]
[Route("/dashboard/workspace/list")]
public class CurrencyController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Add([FromBody] GetListRequest request)
        => this.RequestAsync()
            .For<ListResponse<CurrencyDto>>()
            .With(request);
}
