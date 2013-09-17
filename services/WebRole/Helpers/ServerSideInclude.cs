using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebRole.Helpers
{
    public static class ServerSideInclude
    {
        public static IHtmlString Include(this HtmlHelper helper, string serverPath)
        {
            var filePath = HttpContext.Current.Server.MapPath(serverPath);

            var markup = File.ReadAllText(filePath);
            return new HtmlString(markup);
        }
    }
}