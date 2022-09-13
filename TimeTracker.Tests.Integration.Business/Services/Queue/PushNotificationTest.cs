using Autofac;
using TimeTracker.Business.Notifications.Senders;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Queue;

public class PushNotificationTest: BaseTest
{
    private readonly IQueueService _queueService;

    public PushNotificationTest(): base()
    {
        _queueService = Scope.Resolve<IQueueService>();
    }

    [Fact]
    public async Task ShouldProduce()
    {
        var testContext = new TestNotificationContext()
        {
            ToAddress = "test@test.com"
        };

        await _queueService.PushNotificationAsync(testContext);
    }
}
