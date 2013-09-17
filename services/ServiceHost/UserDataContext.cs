using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

using ServiceEntities.UserData;
using ServiceHost;

namespace ServiceHost
{
    // You can add custom code to this file. Changes will not be overwritten.
    // 
    // If you want Entity Framework to drop and regenerate your database
    // automatically whenever you change your model schema, add the following
    // code to the Application_Start method in your Global.asax file.
    // Note: this will destroy and re-create your database with every model change.
    // 
    // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<ServiceHost.UserDataContext>());
    public class UserDataContext : DbContext
    {
        public UserDataContext()
            : base("name=" + HostEnvironment.UserDataConnection)
        {
        }
        public UserDataContext(string connection) : base(connection) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) { }

        public DbSet<Device> Devices { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<WebSession> WebSessions { get; set; }

        public static Device CreateDeviceFromSession(WebSession session)
        {
            var device = session.Device ?? new Device();
            device.DeviceId = device.DeviceId ?? session.DeviceId;
            device.IpAddress = device.IpAddress ?? device.DeviceId;
            device.Hostname = device.Hostname ?? device.IpAddress;
            device.Name = device.Name ?? device.Hostname;
            device.UserId = device.UserId ?? session.UserId;
            return device;
        }

        public static bool InitializeDatabase()
        {
            TraceLog.TraceDetail("Checking if the UserData database exists");

            Database.SetInitializer<UserDataContext>(null);
            try
            {
                using (var context = new UserDataContext())
                {
                    if (!context.Database.Exists())
                    {
                        // Create the database without Entity Framework migration schema
                        ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        TraceLog.TraceInfo("Created UserData database");
                    }
                    else
                    {
                        try
                        {
                            // make sure the collection is available
                            var list = context.Devices.ToList();
                        }
                        catch (System.Data.EntityCommandExecutionException ex)
                        {
                            TraceLog.TraceException("The UserData database exists but doesn't contain the Devices table", ex);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("The UserData database could not be initialized", ex);
                return false;
            }
        }

        public static void InitializeNewUserAccount(string userName)
        {
            try
            {
                var context = new UserDataContext();

                // add the user
                var person = new Person()
                {
                    Name = userName,
                    UserId = userName,
                    Birthdate = null,
                    IsChild = false
                };
                context.People.Add(person);
                context.SaveChanges();

#if DEBUG
                // add a dummy device for testing
                var device = Device.CreateNewDevice(userName);
                device.PersonId = person.PersonId;
                context.Devices.Add(device);
                context.SaveChanges();
#endif
            }
            catch (Exception ex)
            {
                TraceLog.TraceException(string.Format("UserData initialization failed for user {0}", userName), ex);
                throw;
            }
        }
    }
}