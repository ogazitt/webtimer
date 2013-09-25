using ServiceHost;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using WebMatrix.WebData;
using WebRole.Models;

namespace WebRole.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeTodoSchemaAttribute : ActionFilterAttribute
    {
        private static TodoSchemaInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure Todo database is initialized only once per app start
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        private class TodoSchemaInitializer
        {
            public TodoSchemaInitializer()
            {
                Database.SetInitializer<TodoItemContext>(null);

                try
                {
                    using (var context = new TodoItemContext())
                    {
                        if (!context.Database.Exists())
                        {
                            // Create the Todo database without Entity Framework migration schema
                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                            TraceLog.TraceInfo("Created Todo database");
                        }
                    }
                }
                catch (Exception ex)
                {
                    TraceLog.TraceException("The Todo database could not be initialized", ex);
                    throw new InvalidOperationException("The Todo database could not be initialized", ex);
                }
            }
        }
    }
}
