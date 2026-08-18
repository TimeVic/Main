using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Public.SharedClientReports;

[ApiController]
[Route("/public/shared/report/client")]
public class PublicSharedClientReportController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpGet("{token}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Get(string token)
        => this.RequestAsync()
            .For<GetSharedClientReportResponse>()
            .With(new GetSharedClientReportRequest { Token = token });

    [HttpGet("{token}/get-tasks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetTasks(string token)
        => this.RequestAsync()
            .For<GetSharedClientReportTasksResponse>()
            .With(new GetSharedClientReportTasksRequest { Token = token });
}
