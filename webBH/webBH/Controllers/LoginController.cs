using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using webBH.Models;

namespace webBH.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        private webBHEntities db = new webBHEntities();
        public ActionResult Index()
        {

            if (Session["UserId"] != null && Session["UserEmail"] != null)
            {
                // Người dùng đã đăng nhập, hiển thị trang thông tin cá nhân

                return RedirectToAction("index", "home");
            }
            else
            {
                // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return View();
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "email,password")] User user)
        {
            var find_user = db.Users.FirstOrDefault(u => u.email == user.email);
            if (find_user != null )
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(user.password));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    if (builder.ToString() == find_user.password)
                    {
                        // Đăng nhập thành công
                        // Thực hiện các tác vụ cần thiết
                        Session["UserId"] = find_user.id;
                        Session["UserEmail"] = find_user.email;
                        Session["UserName"] = find_user.name;
                        Session["UserRole"] = find_user.Role.name;
                        Session["RoleId"] = find_user.id_roles;
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ModelState.AddModelError("", "Invalid username or password");
            return View(find_user);
        }
    }
 
}