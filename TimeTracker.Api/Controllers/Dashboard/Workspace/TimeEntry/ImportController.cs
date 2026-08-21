using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace.TimeEntry;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Mvc.Controllers;
using TimeTracker.Business.Services.Storage;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.TimeEntry;

[ApiController]
[Authorize]
[Route("/dashboard/workspace/time-entry/[controller]")]
public class ImportController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost]
    [RequestSizeLimit(FileStorage.MaxFileSize)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Import([FromForm] ImportRequest request)
        => this.RequestAsync()
            .For<ImportResponse>()
            .With(request);
}
