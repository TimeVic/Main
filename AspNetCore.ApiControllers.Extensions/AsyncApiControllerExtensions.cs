using System;
using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.ApiControllers.Extensions
{
    public static class AsyncApiControllerExtensions
    {
        public static Task<IActionResult> RequestAsync<TApiController, TRequest>(
            this TApiController apiController,
            TRequest request
        )
            where TApiController : 
                ControllerBase, 
                IAsyncApiController, 
                IHasDefaultSuccessActionResult, 
                IHasDefaultFailActionResult, 
                IHasInvalidModelStateActionResult,
                IShouldPerformCommit
            where TRequest : IRequest
            => RequestAsync(
                apiController,
                request,
                apiController.Success,
                apiController.Fail
            );

        public static Task<IActionResult> RequestAsync<TApiController, TRequest>(
            this TApiController apiController,
            TRequest request,
            Func<IActionResult> success)
            where TApiController : 
                ControllerBase, 
                IAsyncApiController, 
                IHasDefaultFailActionResult, 
                IHasInvalidModelStateActionResult,
                IShouldPerformCommit
            where TRequest : IRequest
            => RequestAsync(
                apiController,
                request,
                success,
                apiController.Fail);

        public static async Task<IActionResult> RequestAsync<TApiController, TRequest>(
            this TApiController apiController,
            TRequest request,
            Func<IActionResult> success,
            Func<Exception, IActionResult> fail
        )
            where TApiController : 
                ControllerBase,
                IAsyncApiController, 
                IHasInvalidModelStateActionResult,
                IShouldPerformCommit
            where TRequest : IRequest
        {
            try
            {
                if (apiController == null)
                    throw new ArgumentNullException(nameof(apiController));

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (!apiController.ModelState.IsValid)
                    return apiController.InvalidModelState(apiController.ModelState);
                
                await apiController.AsyncRequestBuilder.ExecuteAsync(request);
                if (apiController.CommitPerformer != null)
                {
                    await apiController.CommitPerformer.PerformCommitAsync();
                }
                
                return success();
            }
            catch (Exception exception)
            {
                return fail(exception);
            }
        }
        
        public static Task<IActionResult> RequestAsync<TApiController, TRequest, TResponse>(
            this TApiController apiController,
            TRequest request)
            where TApiController : 
                ControllerBase, 
                IAsyncApiController, 
                IHasDefaultResponseSuccessActionResult, 
                IHasDefaultFailActionResult, 
                IHasInvalidModelStateActionResult,
                IShouldPerformCommit
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
            => RequestAsync(
                apiController,
                request,
                apiController.ResponseSuccess<TResponse>(),
                apiController.Fail);

        public static Task<IActionResult> RequestAsync<TApiController, TRequest, TResponse>(
            this TApiController apiController,
            TRequest request,
            Func<TResponse, IActionResult> success)
            where TApiController : 
                ControllerBase, 
                IAsyncApiController, 
                IHasDefaultFailActionResult, 
                IHasInvalidModelStateActionResult,
                IShouldPerformCommit
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
            => RequestAsync(
                apiController,
                request,
                success,
                apiController.Fail
            );

        public static async Task<IActionResult> RequestAsync<TApiController, TRequest, TResponse>(
            this TApiController apiController,
            TRequest request,
            Func<TResponse, IActionResult> success,
            Func<Exception, IActionResult> fail
        )
            where TApiController : 
                ControllerBase, 
                IAsyncApiController, 
                IHasInvalidModelStateActionResult,
                IShouldPerformCommit
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            if (apiController == null)
                throw new ArgumentNullException(nameof(apiController));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!apiController.ModelState.IsValid)
                return apiController.InvalidModelState(apiController.ModelState);

            try
            {
                var response = await apiController.AsyncRequestBuilder.ExecuteAsync<TRequest, TResponse>(request);
                if (apiController.CommitPerformer != null)
                {
                    await apiController.CommitPerformer.PerformCommitAsync();
                }

                return success(response);
            }
            catch (Exception exception)
            {
                return fail(exception);
            }
        }
    }
}
