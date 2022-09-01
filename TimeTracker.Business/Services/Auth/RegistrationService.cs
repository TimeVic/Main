using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Exceptions.Api;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;

namespace TimeTracker.Business.Services.Auth;

public class RegistrationService: IRegistrationService
{
    private readonly IUserDao _userDao;
    private readonly IQueueService _queueService;

    public RegistrationService(IUserDao userDao, IQueueService queueService)
    {
        _userDao = userDao;
        _queueService = queueService;
    }

    public async Task<UserEntity> CreatePendingUser(string email)
    {
        var existsUser = await _userDao.GetByEmail(email);
        if (existsUser != null)
        {
            throw new RecordIsExistsException();
        }
        var user = await _userDao.CreatePendingUser(email);
        await _queueService.PushNotification(new RegistrationNotificationContext()
        {
            ToAddress = user.Email,
            VerificationToken = user.VerificationToken
        });
        return user;
    }
    
    public async Task<UserEntity> ActivateUser(string verificationToken, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        var user = await _userDao.GetByVerificationToken(verificationToken);
        if (user == null)
        {
            throw new RecordNotFoundException();
        }
        user.VerificationTime = DateTime.UtcNow;
        user.VerificationToken = null;

        user.PasswordSalt = SecurityUtil.GenerateSalt(32);
        user.PasswordHash = SecurityUtil.GeneratePasswordHash(password, user.PasswordSalt);
        
        await _queueService.PushNotification(new EmailVerifiedNotificationContext()
        {
            ToAddress = user.Email
        });
        return user;
    }
}
