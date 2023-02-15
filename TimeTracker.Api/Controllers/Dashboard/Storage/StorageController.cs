using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Dto.RequestsAndResponses.Storage;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;

namespace TimeTracker.Api.Controllers.Dashboard.Storage;

[ApiController]
[Authorize]
[Route("/dashboard/[controller]")]
public class StorageController : MainApiControllerBase
{
    public StorageController(
        IAsyncRequestBuilder asyncRequestBuilder, 
        IDbSessionProvider commitPerformer,
        ILogger<StorageController> logger
    ) : base(asyncRequestBuilder, commitPerformer, logger)
    {
    }

    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Upload([FromForm] UploadRequest request)
        => this.RequestAsync()
            .For<StoredFileDto>()
            .With(request);
    
    [HttpGet("file/{FileId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetFile([FromRoute] GetFileRequest request)
        => this.RequestAsync()
            .For<FileResponse>()
            .With(request);
}
