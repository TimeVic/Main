using Microsoft.Extensions.Configuration;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Queue;

namespace TimeTracker.Business.Services.Auth;

public class ResetPasswordService: IResetPasswordService
{
    private readonly IUserResetPasswordRequestDao _resetPasswordRequestDao;
    private readonly IPasswordService _passwordService;
    private readonly IQueueService _queueService;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly string? _frontendUrl;

    public ResetPasswordService(
        IUserResetPasswordRequestDao resetPasswordRequestDao,
        IPasswordService passwordService,
        IQueueService queueService,
        IConfiguration configuration,
        IDbSessionProvider sessionProvider
    )
    {
        _resetPasswordRequestDao = resetPasswordRequestDao;
        _passwordService = passwordService;
        _queueService = queueService;
        _sessionProvider = sessionProvider;
        _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
    }

    public async Task<UserResetPasswordRequestEntity?> Generate(UserEntity user)
    {
        var actualRequest = await _resetPasswordRequestDao.GetLast(user);
        if (actualRequest != null)
        {
            if (!actualRequest.IsExpired)
            {
                throw new TooManyRequestsException("Wait a while to repeat");
            }
            actualRequest = await _resetPasswordRequestDao.GenerateNew(user);
        }
        else
        {
            actualRequest = await _resetPasswordRequestDao.GenerateNew(user);
        }

        await _queueService.PushNotificationAsync(new ResetPasswordNotificationContext()
        {
            FrontendUrl = _frontendUrl,
            VerificationToken = actualRequest.VerificationToken!,
            ToAddress = actualRequest.User.Email
        });
        return actualRequest;
    }
    
    public async Task ChangePassword(string token, string password)
    {
        var actualRequest = await _resetPasswordRequestDao.GetByToken(token);
        if (actualRequest == null)
        {
            throw new RecordNotFoundException();
        }
        if (actualRequest.IsExpired)
        {
            throw new RecordExpiredException();
        }
        actualRequest.User = _passwordService.SetUserPassword(actualRequest.User, password);
        await _sessionProvider.CurrentSession.SaveAsync(actualRequest.User);
        
        actualRequest.ExpirationTime = DateTime.UtcNow.AddMinutes(-1);
        await _sessionProvider.CurrentSession.SaveAsync(actualRequest);
        
        await _queueService.PushNotificationAsync(new PasswordHasBeenChangedNotificationContext()
        {
            ToAddress = actualRequest.User.Email
        });
    }
}
