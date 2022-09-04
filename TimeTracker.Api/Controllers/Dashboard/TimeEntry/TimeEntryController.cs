﻿using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry;

[ApiController]
[Authorize]
[Route("/dashboard/time-entry")]
public class TimeEntry : MainApiControllerBase
{
    public TimeEntry(
        IAsyncRequestBuilder asyncRequestBuilder, 
        IDbSessionProvider commitPerformer,
        ILogger<TimeEntry> logger
    ) : base(asyncRequestBuilder, commitPerformer, logger)
    {
    }

    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Start([FromBody] StartRequest request)
        => this.RequestAsync()
            .For<TimeEntryDto>()
            .With(request);
    
    [HttpPost("stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Stop([FromBody] StopRequest request)
        => this.RequestAsync()
            .For<TimeEntryDto>()
            .With(request);
}
