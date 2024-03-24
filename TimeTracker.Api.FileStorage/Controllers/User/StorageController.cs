using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Extensions;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.Storage;
using TimeTracker.Business.Common.Mvc.Controllers;

namespace TimeTracker.Api.FileStorage.Controllers.User;

[ApiController]
[Route("/[controller]")]
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
    public Task<IActionResult> Login([FromBody] UploadRequest request)
        => this.RequestAsync()
            .For<UploadResponse>()
            .With(request);
}
