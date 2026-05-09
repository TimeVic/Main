using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class ResetPasswordStep1RequestHandler : IAsyncRequestHandler<ResetPasswordStep1Request>
    {
        private readonly IRegistrationService _registrationService;
        private readonly IResetPasswordService _resetPasswordService;
        private readonly IUserDao _userDao;

        public ResetPasswordStep1RequestHandler(
            IRegistrationService registrationService,
            IResetPasswordService resetPasswordService,
            IUserDao userDao
        )
        {
            _registrationService = registrationService;
            _resetPasswordService = resetPasswordService;
            _userDao = userDao;
        }
    
        public async Task ExecuteAsync(ResetPasswordStep1Request request)
        {
            var user = await _userDao.GetByEmail(request.Email);
            RecordNotFoundException.ThrowIfNull(user);
            await _resetPasswordService.Generate(user);
        }
    }
}
