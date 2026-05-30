using Microsoft.Extensions.Configuration;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
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
    private readonly IPasswordService _passwordService;
    private readonly ILanguageDao _languageDao;
    private readonly string _frontendUrl = string.Empty;

    public RegistrationService(
        IUserDao userDao,
        IQueueService queueService,
        IConfiguration configuration,
        IWorkspaceDao workspaceDao,
        IWorkspaceAccessService workspaceAccessService,
        IPasswordService passwordService,
        ILanguageDao languageDao
    )
    {
        _userDao = userDao;
        _queueService = queueService;
        _workspaceDao = workspaceDao;
        _workspaceAccessService = workspaceAccessService;
        _passwordService = passwordService;
        _languageDao = languageDao;
        _frontendUrl = configuration.GetValue<string>("App:FrontendUrl")!;
    }

    public async Task<UserEntity> CreatePendingUser(string email, string? languageCode = null)
    {
        var existsUser = await _userDao.GetByEmail(email);
        if (existsUser is { IsActivated: true })
        {
            throw new RecordIsExistsException();
        }
        var user = existsUser ?? await _userDao.CreatePendingUser(email);

        await ApplyRegistrationLanguageAsync(user, languageCode);
        await EnsureDefaultWorkspaceAsync(user);
        await SendRegistrationNotificationAsync(user);
        return user;
    }

    public async Task<UserEntity> CreateActivatedUserForSocialLogin(string email, string? userName = null, string? languageCode = null)
    {
        var existsUser = await _userDao.GetByEmail(email);
        var user = existsUser ?? await _userDao.CreatePendingUser(email);

        if (string.IsNullOrWhiteSpace(user.UserName) && !string.IsNullOrWhiteSpace(userName))
        {
            user.UserName = userName.Trim();
        }

        user.VerificationTime ??= DateTime.UtcNow;
        user.VerificationToken = null;
        user.UpdatedAt = DateTime.UtcNow;

        await ApplyRegistrationLanguageAsync(user, languageCode);
        await EnsureDefaultWorkspaceAsync(user);
        return user;
    }

    private async Task ApplyRegistrationLanguageAsync(UserEntity user, string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return;
        }

        var language = await _languageDao.GetByCodeAsync(languageCode) ?? await _languageDao.GetDefaultAsync();
        user.Language = language;
        user.UpdatedAt = DateTime.UtcNow;
    }

    private async Task EnsureDefaultWorkspaceAsync(UserEntity user)
    {
        if (user.CreatedWorkspaces.Any(item => item.IsDefault))
        {
            return;
        }

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
    }

    private async Task SendRegistrationNotificationAsync(UserEntity user)
    {
        await _queueService.PushNotificationAsync(new RegistrationNotificationItemContext(
            user.Email,
            _frontendUrl,
            user.VerificationToken!
        ));
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
        user = _passwordService.SetUserPassword(user, password);
        
        await _queueService.PushNotificationAsync(new EmailVerifiedNotificationItemContext()
        {
            ToAddress = user.Email
        });
        return user;
    }
}
