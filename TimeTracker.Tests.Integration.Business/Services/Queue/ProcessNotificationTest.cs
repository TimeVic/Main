using Autofac;
using TimeTracker.Business.Notifications.Senders;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Queue;

public class ProcessNotificationTest: BaseTest
{
    private readonly IQueueService _queueService;

    public ProcessNotificationTest(): base()
    {
        _queueService = Scope.Resolve<IQueueService>();
    }

    [Fact]
    public async Task ShouldProcessNotification()
    {
        var testContext = new TestNotificationContext()
        {
            ToAddress = "test@test.com"
        };

        await _queueService.PushNotification(testContext);

        var actualProcessedCounter = await _queueService.Process(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(EmailSendingService.IsEmailSent);
        var actualEmail = EmailSendingService.SentMessages.FirstOrDefault();
        Assert.Contains(testContext.ToAddress, actualEmail.To);
    }
}
