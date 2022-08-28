using System;
using Api.Requests.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Persistence.Transactions.Behaviors;

namespace AspNetCore.ApiControllers.Abstractions
{
    [ApiController]
    public class ApiControllerBase
        : ControllerBase,
            IAsyncApiController,
            IHasDefaultSuccessActionResult,
            IHasDefaultResponseSuccessActionResult,
            IHasDefaultFailActionResult,
            IHasInvalidModelStateActionResult,
            IShouldPerformCommit
    {
        private readonly IAsyncRequestBuilder _asyncRequestBuilder;
        private readonly IDbSessionProvider _commitPerformer;
        
        public ApiControllerBase(
            IAsyncRequestBuilder asyncRequestBuilder,
            IDbSessionProvider commitPerformer
        )
        {
            _asyncRequestBuilder = asyncRequestBuilder ?? throw new ArgumentNullException(nameof(asyncRequestBuilder));
            _commitPerformer = commitPerformer ?? throw new ArgumentNullException(nameof(commitPerformer));
        }

        public ApiControllerBase(IAsyncRequestBuilder asyncRequestBuilder)
        {
            _asyncRequestBuilder = asyncRequestBuilder ?? throw new ArgumentNullException(nameof(asyncRequestBuilder));
        }

        public virtual Func<IActionResult> Success
            => () => new OkResult();

        public virtual Func<TResponse, IActionResult> ResponseSuccess<TResponse>()
            where TResponse : IResponse
            => (TResponse response) => new OkObjectResult(response);

        public virtual Func<Exception, IActionResult> Fail
            => (Exception exception) => new BadRequestObjectResult(
                new BadResponseModel(exception)
            );

        public virtual Func<ModelStateDictionary, IActionResult> InvalidModelState
            => (ModelStateDictionary modelState) => new BadRequestObjectResult(new ValidationProblemDetails(modelState).Errors);
        
        IAsyncRequestBuilder IAsyncApiController.AsyncRequestBuilder => _asyncRequestBuilder;

        IDbSessionProvider IShouldPerformCommit.CommitPerformer => _commitPerformer;
    }
}
