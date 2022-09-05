namespace TimeTracker.Web.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToFullDateTime(this DateTime time)
        {
            return time.ToString("MM/dd/yyyy HH:mm:ss");
        }
    }
}