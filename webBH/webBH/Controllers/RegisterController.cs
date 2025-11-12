using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using webBH.Models;

namespace webBH.Controllers
{
    public class RegisterController : Controller
    {
        private webBHEntities db = new webBHEntities();

        // GET: Register
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

        // POST: Register/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "id,name,birthday,sex,email,password")] User user)
            {
            Console.WriteLine(user.sex);
            if (ModelState.IsValid)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    // Băm mật khẩu dưới dạng mảng byte
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(user.password));

                    // Chuyển đổi mảng byte sang chuỗi hexa và lưu vào cơ sở dữ liệu
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    user.password = builder.ToString();
                }

                user.id_roles = 1; // id_role 1 la user
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index","Login");
            }

            ViewBag.id_roles = new SelectList(db.Roles, "id", "name", user.id_roles);
            return View(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
