using System;
using System.Linq;
using System.Data.Entity.Infrastructure;
using System.Security;
using System.Security.Principal;
using Breeze.WebApi;

// ReSharper disable InconsistentNaming
namespace WebRole.Models
{
    public class UserRepository : EFContextProvider<UserContext>
    {
        public UserRepository(IPrincipal user)
        {
            UserId = user.Identity.Name;
        }

        public string UserId { get; private set; }

        public DbQuery<Device> Devices
        {
            get
            {
                return (DbQuery<Device>)Context.Devices
                    .Where(d => d.UserId == UserId);
            }
        }

        public DbQuery<Person> People
        {
            get
            {
                return (DbQuery<Person>)Context.People
                    .Where(p => p.UserId == UserId);
            }
        }

        #region Save processing

        // Todo: delegate to helper classes when it gets more complicated

        protected override bool BeforeSaveEntity(EntityInfo entityInfo)
        {
            var entity = entityInfo.Entity;
            if (entity is Device)
            {
                return BeforeSaveDevice(entity as Device, entityInfo);
            }
            if (entity is Person)
            {
                return BeforeSavePerson(entity as Person, entityInfo);
            }
            throw new InvalidOperationException("Cannot save entity of unknown type");
        }

        private bool BeforeSaveDevice(Device device, EntityInfo info)
        {
            if (info.EntityState == EntityState.Added)
            {
                device.UserId = UserId;
                return true;
            }

            if (UserId != device.UserId)
                throwCannotSaveEntityForThisUser();

            var person = ValidationContext.People.Find(device.PersonId);
            return (null == person)
                       ? throwCannotFindParentPerson()
                       : UserId == person.UserId || throwCannotSaveEntityForThisUser();
        }

        private bool BeforeSavePerson(Person person, EntityInfo info)
        {
            if (info.EntityState == EntityState.Added)
            {
                person.UserId = UserId;
                return true;
            }
            
            return UserId == person.UserId || throwCannotSaveEntityForThisUser();
        }

        // "this.Context" is reserved for Breeze save only!
        // A second, lazily instantiated DbContext will be used
        // for db access during custom save validation. 
        // See this stackoverflow question and reply for an explanation:
        // http://stackoverflow.com/questions/14517945/using-this-context-inside-beforesaveentity
        private UserContext ValidationContext
        {
            get { return _validationContext ?? (_validationContext = new UserContext()); }
        }
        private UserContext _validationContext;

        private bool throwCannotSaveEntityForThisUser()
        {
            throw new SecurityException("Unauthorized user");
        }

        private bool throwCannotFindParentPerson()
        {
            throw new InvalidOperationException("Invalid Device - cannot find Person");
        }

        #endregion

    }
}