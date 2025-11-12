using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using webBH.Models;
using System.Data.Entity;
using System.Net;
using PagedList;

namespace webBH.Controllers
{
    public class HomeController : Controller
    {
        private webBHEntities db = new webBHEntities();
        public ActionResult Index(string currentFilter, int ?page,  int id_category = 0, string SearchString = "")
        {

            if (SearchString != "")
            {
                page = 1;
                var products = db.Products.Include(s => s.Category).Where(x => x.name.ToUpper().Contains(SearchString.ToUpper()));
                return View(products.ToList());
            }
            else
            {
                //SearchString = currentFilter;
                var products = db.Products.Include(s => s.Category);
                return View(products.ToList());
            }

            ViewBag.CurrentFilter = currentFilter;
            //if(id_category == 0)
            //{
            //    int pageSize = 8;
            //    int pageNumber = (page ?? 1);
            //    var products = db.Products.Include(s => s.Category).OrderBy(x => x.name);
            //    return View(products.ToPagedList(pageNumber, pageSize));
            //}
            //else
            //{
            //    var products = db.Products.Include(s => s.Category).Where(x => x.id_category == id_category);
            //    return View(products.ToList());
            //}
            if (Session["UserId"] != null && Session["UserEmail"] != null)
            {
                var products = db.Products.Include(p => p.Category);
                // Người dùng đã đăng nhập, hiển thị trang thông tin cá nhân
                int userId = Convert.ToInt32(Session["UserId"]);
                string username = Session["UserEmail"].ToString();

                // Lấy thông tin người dùng từ CSDL bằng UserId hoặc Username
                User user = db.Users.Find(userId);

                return View( products.ToList());
            }
            else
            {
                // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("index", "login");
            }

        }
        public ActionResult Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}