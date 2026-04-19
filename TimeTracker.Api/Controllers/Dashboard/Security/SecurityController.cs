using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Security;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard.Security;

[ApiController]
[Authorize]
[Route("/dashboard/[controller]")]
public class SecurityController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost("permissions/workspace")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetWorkspacePermissions(
        [FromBody] GetWorkspacePermissionsRequest request
    )
        => this.RequestAsync()
            .For<GetWorkspacePermissionsResponse>()
            .With(request);
}
