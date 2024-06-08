using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.FileStorage.Dto.Entities;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.Directory;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Business.FileStorage.Mvc.Controllers;

namespace TimeTracker.Api.FileStorage.Controllers.Directory;

[ApiController]
[Route("/[controller]")]
public class DirectoryController: BaseStorageController
{
    public DirectoryController(ILifetimeScope scope): base(scope)
    {
    }

    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Get([FromRoute] GetFilesRequest request)
        => this.RequestAsync()
            .For<PaginatedListDto<FileStorageFileDto>>()
            .With(request);
}
