using ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Security;
using WebMatrix.WebData;
using WebRole.Controllers;
using WebRole.Helpers;

namespace WebRole.Filters
{
    public class BasicAuthAttribute : System.Web.Http.AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var principal = AuthHelper.Authenticate(actionContext);
            if (principal != null)
            {
                var controller = actionContext.ControllerContext.Controller as CollectorController;
                if (controller != null)
                    controller.CurrentUser = principal;
                return;
            }

            base.HandleUnauthorizedRequest(actionContext);
        }
    }
}