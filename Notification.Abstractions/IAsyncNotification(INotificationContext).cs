using System.Threading;
using System.Threading.Tasks;

namespace Notification.Abstractions
{
    public interface IAsyncNotification<in TCommandcNotification> where TCommandcNotification : INotificationContext
    {
        Task SendAsync(TCommandcNotification commandContext, CancellationToken cancellationToken = default);
    }
}