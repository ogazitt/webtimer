using MongoRepository;
using WebTimer.ServiceEntities.SiteMap;

namespace WebTimer.ServiceHost
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

        public static CollectorContext CollectorContextFor(string user)
        {
            return new CollectorContext(user); 
        }

        public static UserDataContext NewUserDataContext
        {
            get { return new UserDataContext(); }
        }

        public static SiteMapRepository NewSiteMapRepository
        {
            get { return new SiteMapRepository(
                new MongoRepository<SiteMapping>(HostEnvironment.MongoUri),
                new MongoRepository<SiteExpression>(HostEnvironment.MongoUri),
                new MongoRepository<UnknownSite>(HostEnvironment.MongoUri),
                new MongoRepository<SiteMapVersion>(HostEnvironment.MongoUri));
            }
        }

        public static UserDataContext StaticUserDataContext
        {   // use a static context to access static data (serving values out of EF cache)
            get
            {
#if DEBUG
                // if in a debug build, always go to the database
                return new UserDataContext();
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