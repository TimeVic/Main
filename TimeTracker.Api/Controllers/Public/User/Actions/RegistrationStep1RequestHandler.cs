using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class RegistrationStep1RequestHandler : IAsyncRequestHandler<RegistrationStep1Request>
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationStep1RequestHandler(
            IRegistrationService registrationService
        )
        {
            _registrationService = registrationService;
        }
    
        public async Task ExecuteAsync(RegistrationStep1Request request)
        {
            await _registrationService.CreatePendingUser(request.Email);
        }
    }
}
