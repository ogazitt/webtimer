﻿using System;
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
    public class DashboardRepository : EFContextProvider<UserDataContext>
    {
        public DashboardRepository(IPrincipal user)
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

        public DbQuery<WebSession> WebSessions
        {
            get
            {
                return (DbQuery<WebSession>)Context.WebSessions
                    .Where(w => w.UserId == UserId);
            }
        }

        #region Save processing

        // Todo: delegate to helper classes when it gets more complicated

        protected override bool BeforeSaveEntity(EntityInfo entityInfo)
        {
            var entity = entityInfo.Entity;
            if (entity is Person)
            {
                return BeforeSavePerson(entity as Person, entityInfo);
            }
            if (entity is Device)
            {
                return BeforeSaveDevice(entity as Device, entityInfo);
            }
            throw new InvalidOperationException("Cannot save entity of unknown type");
        }


        private bool BeforeSavePerson(Person person, EntityInfo info)
        {
            if (info.EntityState == EntityState.Added)
            {
                person.UserId = UserId;
                return true;
            }
            return UserId == person.UserId || throwCannotSaveEntityForThisUser(person.UserId);
        }

        private bool BeforeSaveDevice(Device device, EntityInfo info)
        {
            var person = device.PersonId.HasValue ? ValidationContext.People.Find(device.PersonId) : null;
            return (null != person)
                       ? UserId == person.UserId || throwPersonDoesNotBelongToThisUser(person.UserId)
                       : UserId == device.UserId || throwCannotSaveEntityForThisUser(device.UserId);
        }

        // "this.Context" is reserved for Breeze save only!
        // A second, lazily instantiated DbContext will be used
        // for db access during custom save validation. 
        // See this stackoverflow question and reply for an explanation:
        // http://stackoverflow.com/questions/14517945/using-this-collectorContext-inside-beforesaveentity
        private UserDataContext ValidationContext
        {
            get { return _validationContext ?? (_validationContext = new UserDataContext()); }
        }
        private UserDataContext _validationContext;

        private bool throwCannotSaveEntityForThisUser(string userId)
        {
            var message = string.Format("UserId of entity {0} does not match logged in User {1}", userId, UserId);
            TraceLog.TraceError(message);
            throw new SecurityException(message);
        }

        private bool throwPersonDoesNotBelongToThisUser(string userId)
        {
            var message = string.Format("UserId of device's person entity {0} does not match logged in User {1}", userId, UserId);
            TraceLog.TraceError(message);
            throw new InvalidOperationException(message);
        }

        #endregion

    }
}