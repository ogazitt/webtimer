using System;
using System.Linq;
using System.Data.Entity.Infrastructure;
using System.Security;
using System.Security.Principal;
using Breeze.WebApi;

using ServiceHost;
using ServiceEntities.UserData;

// ReSharper disable InconsistentNaming
namespace WebRole.Models
{
    public class UserDataRepository : UserDataContext //, IDisposable
    {
        //UserDataContext _context = null;

        public UserDataRepository(IPrincipal user) : this(user.Identity.Name) { }

        public UserDataRepository(string userName)
        {
            UserId = userName;
            //_context = Storage.NewUserDataContext;
        }

        public string UserId { get; private set; }

        public new DbQuery<Category> Categories
        {
            get
            {
                return (DbQuery<Category>)base.Categories;
            }
        }

        public new DbQuery<Device> Devices
        {
            get
            {
                return (DbQuery<Device>)base.Devices
                    .Where(d => d.UserId == UserId);
            }
        }

        public new DbQuery<Person> People
        {
            get
            {
                return (DbQuery<Person>)base.People
                    .Where(p => p.UserId == UserId);
            }
        }

        public new DbQuery<WebSession> WebSessions
        {
            get
            {
                return (DbQuery<WebSession>)base.WebSessions
                    .Where(w => w.UserId == UserId);
            }
        }

        public void InitializeNewUserAccount()
        {
            try
            {
                // add the user and the "uncategorized" person
                var person = new Person()
                {
                    Name = "Shared",
                    UserId = UserId,
                    Birthdate = null,
                    IsChild = false
                };
                AddPerson(person);
                person = new Person()
                {
                    Name = UserId,
                    UserId = UserId,
                    Birthdate = null,
                    IsChild = false
                };
                AddPerson(person);

#if DEBUG
                // add a dummy device for testing
                var device = Device.CreateNewDevice(UserId);
                device.PersonId = person.PersonId;
                AddDevice(device);
#endif
            }
            catch (Exception ex)
            {
                TraceLog.TraceException(string.Format("UserData initialization failed for user {0}", UserId), ex);
                throw;
            }
        }

        public void AddDevice(Device device)
        {
            if (device == null)
                return;
            base.Devices.Add(device);
            base.SaveChanges();
        }
        
        public void AddPerson(Person person)
        {
            if (person == null)
                return;
            
            // ensure the person belongs to the current user
            person.UserId = UserId;

            if (string.IsNullOrEmpty(person.Color))
            {
                // default to first color
                person.Color = Person.ColorList[0];

                // try to assign a color that hasn't yet been assigned 
                var colors = People.Select(p => p.Color).Distinct();
                for (int i = 0; i < Person.ColorList.Count; i++)
                {
                    if (!colors.Contains(Person.ColorList[i]))
                    {
                        person.Color = Person.ColorList[i];
                        break;
                    }
                }
            }

            base.People.Add(person);
            base.SaveChanges();
        }

        /*
        #region IDisposable

        private bool disposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this._context.Dispose();
                }
            }

            this.disposed = true;
        }

        #endregion
         * */
    }
}