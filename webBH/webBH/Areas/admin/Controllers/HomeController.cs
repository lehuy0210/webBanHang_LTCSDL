using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace webBH.Areas.admin.Controllers
{
    public class HomeController : Controller
    {
        // GET: admin/Home

        [Authorize(Roles ="admin")]
          public ActionResult Index()
        {
                return View();
        }
    }
}