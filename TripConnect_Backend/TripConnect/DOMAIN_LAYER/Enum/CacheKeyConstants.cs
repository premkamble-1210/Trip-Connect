namespace DOMAIN_LAYER.Enum
{
    /// <summary>
    /// Constants for cache keys used throughout the application
    /// </summary>
    public static class CacheKeyConstants
    {
        // User Cache Keys
        public const string USER_BY_ID = "user:id:{0}";
        public const string USER_BY_EMAIL = "user:email:{0}";
        public const string USER_ALL = "user:all";
        public const string USER_PROFILE = "user:profile:{0}";

        // Trip Cache Keys
        public const string TRIP_BY_ID = "trip:id:{0}";
        public const string TRIP_ALL = "trip:all";
        public const string TRIP_BY_USER = "trip:user:{0}";
        public const string TRIP_MEMBERS = "trip:members:{0}";
        public const string TRIP_DETAILS = "trip:details:{0}";

        // Expense Cache Keys
        public const string EXPENSE_BY_ID = "expense:id:{0}";
        public const string EXPENSE_BY_TRIP = "expense:trip:{0}";
        public const string EXPENSE_SUMMARY = "expense:summary:{0}";
        public const string EXPENSE_SPLITS = "expense:splits:{0}";

        // Chat Cache Keys
        public const string CHAT_MESSAGES = "chat:messages:{0}";
        public const string CHAT_HISTORY = "chat:history:{0}:{1}";

        // Join Request Cache Keys
        public const string JOIN_REQUEST_BY_ID = "joinrequest:id:{0}";
        public const string JOIN_REQUEST_BY_TRIP = "joinrequest:trip:{0}";
        public const string JOIN_REQUEST_BY_USER = "joinrequest:user:{0}";

        // Rating Cache Keys
        public const string RATING_BY_ID = "rating:id:{0}";
        public const string RATING_BY_TRIP = "rating:trip:{0}";
        public const string RATING_USER = "rating:user:{0}";

        // Trip Member Cache Keys
        public const string TRIP_MEMBER_BY_ID = "tripmember:id:{0}";
        public const string TRIP_MEMBER_BY_TRIP = "tripmember:trip:{0}";

        // Cache invalidation patterns
        public const string CACHE_INVALIDATE_PATTERN = "{0}:*";
    }
}
