using Microsoft.Extensions.Configuration;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Resources;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Business.Services.Auth;

public class RegistrationService: IRegistrationService
{
    private readonly IUserDao _userDao;
    private readonly IQueueService _queueService;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly string _frontendUrl;

    public RegistrationService(
        IUserDao userDao,
        IQueueService queueService,
        IConfiguration configuration,
        IWorkspaceDao workspaceDao,
        IWorkspaceAccessService workspaceAccessService
    )
    {
        _userDao = userDao;
        _queueService = queueService;
        _workspaceDao = workspaceDao;
        _workspaceAccessService = workspaceAccessService;
        _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
    }

    public async Task<UserEntity> CreatePendingUser(string email)
    {
        var existsUser = await _userDao.GetByEmail(email);
        if (existsUser is { IsActivated: true })
        {
            throw new RecordIsExistsException();
        }
        var user = existsUser ?? await _userDao.CreatePendingUser(email);
        await _queueService.PushNotificationAsync(new RegistrationNotificationItemContext(
            user.Email,
            _frontendUrl,
            user.VerificationToken
        ));
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

        var userName = StringUtils.GetUserNameFromEmail(user.Email);
        var workspaceName = string.Format(
            UserResources.DefaultWorkspaceName,
            userName?.FirstCharToUpper()
        );
        var workspace = await _workspaceDao.CreateWorkspaceAsync(
            user,
            workspaceName,
            true
        );
        await _workspaceAccessService.ShareAccessAsync(workspace, user, MembershipAccessType.Owner);
        
        await _queueService.PushNotificationAsync(new EmailVerifiedNotificationItemContext()
        {
            ToAddress = user.Email
        });
        return user;
    }
}
