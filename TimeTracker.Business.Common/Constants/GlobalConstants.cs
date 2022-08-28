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
    }
}
