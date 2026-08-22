using System.ComponentModel.DataAnnotations;
using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Approval;

[ApiController]
[Authorize]
[Route("/dashboard/time-entry/approval")]
public class TimeEntryApprovalController(ILifetimeScope scope) : MainApiControllerBase(scope)
{

    [HttpPost("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetStatus([FromBody] GetStatusRequest request)
        => this.RequestAsync().For<TimeEntryApprovalStatusSummaryDto>().With(request);

    [HttpPost("submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Submit([FromBody] SubmitRequest request)
        => this.RequestAsync().For<TimeEntryDto>().With(request);

    [HttpPost("submit-period")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> SubmitPeriod([FromBody] SubmitPeriodRequest request)
        => this.RequestAsync().For<PaginatedListDto<TimeEntryDto>>().With(request);

    [HttpPost("unapprove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Unapprove([FromBody] UnapproveRequest request)
        => this.RequestAsync().For<TimeEntryDto>().With(request);

    [HttpPost("approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Approve([FromBody] ApproveRequest request)
        => this.RequestAsync().For<PaginatedListDto<TimeEntryDto>>().With(request);

    [HttpPost("reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Reject([FromBody] RejectRequest request)
        => this.RequestAsync().For<PaginatedListDto<TimeEntryDto>>().With(request);

    [HttpPost("submitters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetSubmitters([FromBody] GetSubmittersRequest request)
        => this.RequestAsync().For<GetSubmittersResponse>().With(request);

    [HttpPost("details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetDetails([FromBody] GetApprovalDetailsRequest request)
        => this.RequestAsync().For<GetApprovalDetailsResponse>().With(request);
}
