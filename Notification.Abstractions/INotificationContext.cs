namespace Notification.Abstractions
{
    public interface INotificationContext
    {
    }

    public static class INotificationContextExtensions
    {
        public static string GetTypeAsString(this INotificationContext context)
        {
            var type = context.GetType();
            return string.Join(
                ".",
                type.Namespace,
                type.Name
            );
        }
    }
}