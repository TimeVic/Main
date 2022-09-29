using TimeTracker.Business.Extensions.Resources;

namespace TimeTracker.Business.Extensions
{
    public static class DateTimeExtensions
    {
        public enum RoundTo
        {
            Second,
            Minute,
            Hour, 
            Day
        }
        
        public static long ToUnixTime(this DateTime d)
        {
            return (long)(
                d - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            ).TotalMilliseconds;
        }

        public static string TimeAgo(this DateTime dt)
        {
            string resultTemplate;
            TimeSpan span = DateTime.UtcNow - dt;
            if (span.Days > 365)
            {
                int years = (span.Days / 365);
                if (span.Days % 365 != 0)
                    years += 1;
                return String.Format(years == 1 ? R.YearAgo : R.YearsAgo, years);
            }

            if (span.Days > 30)
            {
                int months = (span.Days / 30);
                if (span.Days % 31 != 0)
                    months += 1;
                if (months == 1)
                    resultTemplate = R.MonthAgo;
                else if (months >= 2 && months <= 4)
                    resultTemplate = R.MonthsAgoV2;
                else
                    resultTemplate = R.MonthsAgo;
                return String.Format(resultTemplate, months);
            }

            if (span.Days > 0)
            {
                if (span.Days == 1)
                    resultTemplate = R.DayAgo;
                else if (span.Days >= 2 && span.Days <= 4
                         || span.Days >= 22 && span.Days <= 24)
                {
                    resultTemplate = R.DaysAgoV2;
                }
                else
                    resultTemplate = R.DaysAgo;

                return String.Format(resultTemplate, span.Days);
            }

            if (span.Hours > 0)
            {
                if (span.Hours == 1)
                    resultTemplate = R.HourAgo;
                else if (span.Hours >= 2 && span.Hours <= 4
                         || span.Hours >= 22 && span.Hours <= 24)
                {
                    resultTemplate = R.HoursAgoV2;
                }
                else
                    resultTemplate = R.HoursAgo;

                return String.Format(resultTemplate, span.Hours);
            }

            if (span.Minutes > 0)
            {
                if (span.Minutes == 1)
                    resultTemplate = R.MinuteAgo;
                else if (span.Minutes >= 2 && span.Minutes <= 4
                         || span.Minutes >= 22 && span.Minutes <= 24
                         || span.Minutes >= 32 && span.Minutes <= 34
                         || span.Minutes >= 42 && span.Minutes <= 44
                         || span.Minutes >= 52 && span.Minutes <= 54)
                {
                    resultTemplate = R.MinutesAgoV2;
                }
                else
                    resultTemplate = R.MinutesAgo;

                return String.Format(resultTemplate, span.Minutes);
            }

            if (span.Seconds > 5)
            {
                if (span.Seconds >= 22 && span.Seconds <= 24
                    || span.Seconds >= 32 && span.Seconds <= 34
                    || span.Seconds >= 42 && span.Seconds <= 44
                    || span.Seconds >= 52 && span.Seconds <= 54)
                {
                    resultTemplate = R.SecondsAgoV2;
                }
                else
                    resultTemplate = R.SecondsAgo;

                return String.Format(resultTemplate, span.Seconds);
            }

            if (span.Seconds <= 5)
                return R.JustNow;
            return string.Empty;
        }

        public static string ToSimpleFormattedString(this DateTime dt)
        {
            return dt.ToString("hh:mm dd-MM-yyyy");
        }

        public static string ToContentTimeString(this DateTime dt)
        {
            return dt.ToString("MMM dd\\'yy \\a\\t HH:mm");
        }

        public static DateTime Round(this DateTime date, RoundTo roundTo)
        {
            DateTime dtRounded = new DateTime();

            switch (roundTo)
            {
                case RoundTo.Second:
                    dtRounded = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                    if (date.Millisecond >= 500) dtRounded = dtRounded.AddSeconds(1);
                    break;
                case RoundTo.Minute:
                    dtRounded = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                    if (date.Second >= 30) dtRounded = dtRounded.AddMinutes(1);
                    break;
                case RoundTo.Hour:
                    dtRounded = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
                    if (date.Minute >= 30) dtRounded = dtRounded.AddHours(1);
                    break;
                case RoundTo.Day:
                    dtRounded = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                    if (date.Hour >= 12) dtRounded = dtRounded.AddDays(1);
                    break;
            }

            return dtRounded;
        }
        
        public static DateTime StartOfDay(this DateTime theDate)
        {
            return theDate.Date;
        }

        public static DateTime EndOfDay(this DateTime theDate)
        {
            return theDate.Date.AddDays(1).AddTicks(-1);
        }
    }
}
