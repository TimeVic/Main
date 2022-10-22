namespace TimeTracker.Business.Common.Constants
{
    public static class GlobalConstants
    {
        public static bool IsDebugMode
        {
            get
            {
#if DEBUG
                return true;
#endif
                return false;
            }

        }
        
        /// <summary>
        /// Page size
        /// </summary>
        public const int ListPageSize = 20;

        /// <summary>
        /// The maximum number of users who will be given access
        /// </summary>
        public const int ApplicationMaxShares = 20;
        
        public static readonly TimeSpan EndOfDay = TimeSpan.FromHours(23)
            .Add(TimeSpan.FromMinutes(59))
            .Add(TimeSpan.FromSeconds(59))
            .Add(TimeSpan.FromMilliseconds(999));
    }
}
