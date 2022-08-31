using TimeTracker.Business.Exceptions.Api;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;

namespace TimeTracker.Business.Services.Auth;

public class AuthorizationService: IAuthorizationService
{
    private readonly IUserDao _userDao;
    private readonly IQueueService _queueService;

    public AuthorizationService(IUserDao userDao, IQueueService queueService)
    {
        _userDao = userDao;
        _queueService = queueService;
    }

    public async Task<UserEntity> CreatePendingUser(string email)
    {
        var existsUser = await _userDao.GetExistsByUserName(email);
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
}
