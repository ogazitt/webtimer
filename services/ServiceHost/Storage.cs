namespace ServiceHost
{
    // ****************************************************************************
    // static class for getting storage contexts
    // ****************************************************************************
    public static class Storage
    {
#if !DEBUG
        private static UserContext staticUserContext;
#endif

        public static CollectorContext NewCollectorContext
        {
            get { return new CollectorContext(); }
        }

        public static UserContext NewUserContext
        {
            get { return new UserContext(); }
        }

        public static UserContext StaticUserContext
        {   // use a static context to access static data (serving values out of EF cache)
            get
            {
#if DEBUG
                // if in a debug build, always go to the database
                return new UserContext();
#else
                if (staticUserContext == null)
                {
                    staticUserContext = new UserContext();
                }
                return staticUserContext;
#endif
            }
        }
    }
}