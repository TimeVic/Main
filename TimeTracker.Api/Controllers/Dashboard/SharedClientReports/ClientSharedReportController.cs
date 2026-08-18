using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard.SharedClientReports;

[ApiController]
[Authorize]
[Route("/dashboard/report/share/client")]
public class ClientSharedReportController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> SetSettings([FromBody] ClientShareReportSettingsRequest request)
    {
        return this.RequestAsync()
            .For<ClientShareReportSettingsResponse>()
            .With(request);
    }
}
