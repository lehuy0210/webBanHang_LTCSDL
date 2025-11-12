using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using webBH.Models;

namespace webBH.Areas.admin.Controllers
{
    public class ProductsController : Controller
    {
        private webBHEntities db = new webBHEntities();

        // GET: admin/Products
        public ActionResult Index(string currentFilter, int?page)
        {
            ViewBag.CurrentFilter = currentFilter;
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var products = db.Products.Include(p => p.Category).OrderBy(x => x.name); ;
            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // GET: admin/Products/Details/5
        public ActionResult Details(int? id)
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

        // GET: admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.id_category = new SelectList(db.Categories, "id", "name");
            return View();
        }

        // POST: admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,price,size,color,image,id_category")] Product product, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                if(image != null && image.ContentLength > 0)
                {
                    string filename = Path.GetFileName(image.FileName);
                    string path = Server.MapPath("~/Images/" + filename);
                    product.image = "Images/" + filename;
                    image.SaveAs(path);
                }
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_category = new SelectList(db.Categories, "id", "name", product.id_category);
            return View(product);
        }

        // GET: admin/Products/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.id_category = new SelectList(db.Categories, "id", "name", product.id_category);
            return View(product);
        }

        // POST: admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,price,size,color,image,id_category")] Product product, HttpPostedFileBase imageUpload, string imageSP)
        {
            if (ModelState.IsValid)
            {
                if (imageUpload != null && imageUpload.ContentLength > 0)
                {
                    string filename = Path.GetFileName(imageUpload.FileName);
                    string path = Server.MapPath("~/Images/" + filename);
                    product.image = "Images/" + filename;
                    imageUpload.SaveAs(path);
                }
                else
                {
                    product.image = imageSP; 
                }
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_category = new SelectList(db.Categories, "id", "name", product.id_category);
            return View(product);
        }

        // GET: admin/Products/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
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
