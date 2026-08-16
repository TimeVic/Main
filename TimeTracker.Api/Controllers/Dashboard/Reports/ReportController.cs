using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard.Reports;

[ApiController]
[Authorize]
[Route("/dashboard/[controller]")]
public class ReportController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost("summary/personal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> PersonalSummaryReport([FromBody] SummaryReportRequest request)
        => this.RequestAsync()
            .For<SummaryReportResponse>()
            .With(request);

    [HttpPost("summary/team")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> TeamSummaryReport([FromBody] TeamSummaryReportRequest request)
        => this.RequestAsync()
            .For<TeamSummaryReportResponse>()
            .With(request);

    [HttpPost("workspace-financial-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> WorkspaceFinancialSummaryReport([FromBody] WorkspaceFinancialSummaryReportRequest request)
        => this.RequestAsync()
            .For<WorkspaceFinancialSummaryReportResponse>()
            .With(request);

    [HttpPost("user-payments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> UserPaymentReport([FromBody] UserPaymentReportRequest request)
        => this.RequestAsync()
            .For<UserPaymentReportResponse>()
            .With(request);
}
