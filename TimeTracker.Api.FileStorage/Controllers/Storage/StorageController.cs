using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.FileStorage.Dto.Entities;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.Storage;
using TimeTracker.Business.FileStorage.Mvc.Controllers;

namespace TimeTracker.Api.FileStorage.Controllers.User;

[ApiController]
[Route("/[controller]")]
public class StorageController(ILifetimeScope scope) : BaseStorageController(scope)
{
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Upload([FromForm] UploadRequest request)
        => this.RequestAsync()
            .For<FileStorageFileDto>()
            .With(request);
}
