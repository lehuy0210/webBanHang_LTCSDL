using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace webBH.Controllers
{
    public class LogoutController : Controller
    {
        // GET: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index()
        {
            Session.Clear();
            // Xóa tất cả các phiên làm việc và đăng xuất khỏi ứng dụng
            FormsAuthentication.SignOut();

            // Chuyển hướng người dùng đến trang đăng nhập
            return RedirectToAction("Index", "Login");
        }
    }
}