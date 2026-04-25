using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class ResetPasswordStep2RequestHandler : IAsyncRequestHandler<ResetPasswordStep2Request>
    {
        private readonly IResetPasswordService _resetPasswordService;

        public ResetPasswordStep2RequestHandler(
            IResetPasswordService resetPasswordService
        )
        {
            _resetPasswordService = resetPasswordService;
        }
    
        public async Task ExecuteAsync(ResetPasswordStep2Request request)
        {
            await _resetPasswordService.ChangePassword(request.VerficationToken, request.Password);
        }
    }
}
