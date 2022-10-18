using Autofac;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Auth.RegistrationService;

public class ActivateUserTest: BaseTest
{
    private readonly IRegistrationService _registrationService;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IQueueService _queueService;
    private readonly IQueueDao _queueDao;
    private readonly IUserDao _userDao;

    public ActivateUserTest(): base()
    {
        _registrationService = Scope.Resolve<IRegistrationService>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _userDao = Scope.Resolve<IUserDao>();
        _queueService = Scope.Resolve<IQueueService>();
        _queueDao = Scope.Resolve<IQueueDao>();

        _queueDao.CompleteAllPending();
    }

    [Fact]
    public async Task ShouldActivateUserByToken()
    {
        var expectedPassword = "some password";
        var expectedEmail = _userFactory.Generate().Email;
        
        var user = await _registrationService.CreatePendingUser(expectedEmail);
        var activatedUser = await _registrationService.ActivateUser(user.VerificationToken, expectedPassword);
        
        Assert.Null(activatedUser.VerificationToken);
        Assert.NotNull(activatedUser.VerificationTime);

        Assert.NotNull(activatedUser.PasswordSalt);
        var expectedPasswordHash = SecurityUtil.GeneratePasswordHash(expectedPassword, activatedUser.PasswordSalt);
        Assert.Equal(
            expectedPasswordHash,
            activatedUser.PasswordHash
        );
        Assert.True(activatedUser.IsActivated);

        var actualDefaultWorkspace = await _userDao.GetDefaultWorkspace(activatedUser);
        Assert.NotNull(actualDefaultWorkspace);
        Assert.True(actualDefaultWorkspace.IsDefault);
        
        
        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(EmailSendingServiceMock.IsEmailSent);
        var actualEmail = EmailSendingServiceMock.SentMessages.FirstOrDefault();
        Assert.Contains(user.Email, actualEmail.To);
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfNotFound()
    {
        await Assert.ThrowsAsync<RecordNotFoundException>(async () =>
        {
            await _registrationService.ActivateUser("fake token", "fake password");
        });
    }
    
    [Fact]
    public async Task ShouldCreateDefaultWorkspace()
    {
        var expectedPassword = "some password";
        var expectedEmail = _userFactory.Generate().Email;
        
        var user = await _registrationService.CreatePendingUser(expectedEmail);
        var activatedUser = await _registrationService.ActivateUser(user.VerificationToken, expectedPassword);
        await CommitDbChanges();
        
        Assert.Equal(1, activatedUser.Workspaces.Count);
        Assert.Contains(activatedUser.Workspaces, item =>
        {
            var userName = StringUtils.GetUserNameFromEmail(user.Email);
            return item.Name.ToLower().Contains(userName);
        });
        Assert.All(activatedUser.Workspaces, item =>
        {
            Assert.True(item.Id > 0);
        });
    }
}
