using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Dto.RequestsAndResponses.Storage;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Mvc.Controllers;
using TimeTracker.Business.Services.Storage;

namespace TimeTracker.Api.Controllers.Dashboard.Storage;

[ApiController]
[Authorize]
[Route("/dashboard/[controller]")]
public class StorageController(ILifetimeScope scope) : MainApiControllerBase(scope)
{
    [HttpPost("upload")]
    [RequestSizeLimit(FileStorage.MaxFileSize)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Upload([FromForm] UploadRequest request)
        => this.RequestAsync()
            .For<StoredFileDto>()
            .With(request);
    
    [HttpPost("list")]
    [RequestSizeLimit(FileStorage.MaxFileSize)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetList([FromBody] GetListRequest request)
        => this.RequestAsync()
            .For<GetListResponse>()
            .With(request);
    
    [HttpGet("file/{FileId:Guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetFile([FromRoute] Guid fileId, [FromQuery] StorageImageSize? imageSize)
        => this.RequestAsync()
            .For<FileResponse>()
            .With(new GetFileRequest()
            {
                FileId = fileId,
                ImageSize = imageSize
            });
    
    [HttpPost("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task Upload([FromBody] DeleteRequest request)
        => this.RequestAsync(request);
}
