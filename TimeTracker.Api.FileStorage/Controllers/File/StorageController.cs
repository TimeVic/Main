using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.FileStorage.Dto.Entities;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.File;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.Storage;
using TimeTracker.Business.FileStorage.Mvc.Controllers;

namespace TimeTracker.Api.FileStorage.Controllers.File;

[ApiController]
[Route("/[controller]")]
public class FileController(ILifetimeScope scope) : BaseStorageController(scope)
{
    [HttpGet("{bucket:maxlength(255)}/{fileId:maxlength(50)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Get([FromRoute] GetRequest request)
        => this.RequestAsync()
            .For<GetResponse>()
            .With(request);
}
