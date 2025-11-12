using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace webBH.Models
{
    public static class GetSessionValue
    {
        public static MvcHtmlString GetSession(this HtmlHelper htmlHelper, string key)
        {
            var sessionValue = htmlHelper.ViewContext.HttpContext.Session[key];

            return MvcHtmlString.Create(sessionValue != null ? sessionValue.ToString() : string.Empty);
        }
    }
}