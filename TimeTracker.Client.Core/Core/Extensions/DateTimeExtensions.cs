namespace TimeTracker.Client.Core.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToFullDateTime(this DateTime time)
        {
            return time.ToString("MM/dd/yyyy HH:mm:ss");
        }
    }
}
