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
    [HttpPost("payments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> PaymentsReport([FromBody] PaymentReportRequest request)
        => this.RequestAsync()
            .For<PaymentReportResponse>()
            .With(request);
    
    [HttpPost("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> SummaryReport([FromBody] SummaryReportRequest request)
        => this.RequestAsync()
            .For<SummaryReportResponse>()
            .With(request);
}
