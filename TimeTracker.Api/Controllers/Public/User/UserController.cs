using Api.Requests.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using AspNetCore.ApiControllers.Extensions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;

namespace TimeTracker.Api.Controllers.Public.User;

[ApiController]
[Route("/[controller]")]
public class UserController : MainApiControllerBase
{
    public UserController(
        IAsyncRequestBuilder asyncRequestBuilder, 
        IDbSessionProvider commitPerformer,
        ILogger<UserController> logger
    ) : base(asyncRequestBuilder, commitPerformer, logger)
    {
    }

    [HttpPost("registration/step1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> RegistrationStep1([FromBody] RegistrationStep1Request request)
        => this.RequestAsync(request);
        
    [HttpPost("registration/step2")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> RegistrationStep2([FromBody] RegistrationStep2Request request)
        => this.RequestAsync()
            .For<RegistrationStep2ResponseDto>()
            .With(request);
}
