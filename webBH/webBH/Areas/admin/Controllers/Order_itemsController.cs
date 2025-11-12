using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using webBH.Models;

namespace webBH.Areas.admin.Controllers
{
    public class Order_itemsController : Controller
    {
        private webBHEntities db = new webBHEntities();

        // GET: admin/Order_items
        public ActionResult Index()
        {
            var order_items = db.Order_items.Include(o => o.Order).Include(o => o.Product);
            return View(order_items.ToList());
        }

        // GET: admin/Order_items/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order_items order_items = db.Order_items.Find(id);
            if (order_items == null)
            {
                return HttpNotFound();
            }
            return View(order_items);
        }

        // GET: admin/Order_items/Create
        public ActionResult Create()
        {
            ViewBag.id_order = new SelectList(db.Orders, "id", "id");
            ViewBag.id_product = new SelectList(db.Products, "id", "name");
            return View();
        }

        // POST: admin/Order_items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,quanlity,id_order,id_product,total_money")] Order_items order_items)
        {
            if (ModelState.IsValid)
            {
                db.Order_items.Add(order_items);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_order = new SelectList(db.Orders, "id", "id", order_items.id_order);
            ViewBag.id_product = new SelectList(db.Products, "id", "name", order_items.id_product);
            return View(order_items);
        }

        // GET: admin/Order_items/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order_items order_items = db.Order_items.Find(id);
            if (order_items == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_order = new SelectList(db.Orders, "id", "id", order_items.id_order);
            ViewBag.id_product = new SelectList(db.Products, "id", "name", order_items.id_product);
            return View(order_items);
        }

        // POST: admin/Order_items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,quanlity,id_order,id_product,total_money")] Order_items order_items)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order_items).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_order = new SelectList(db.Orders, "id", "id", order_items.id_order);
            ViewBag.id_product = new SelectList(db.Products, "id", "name", order_items.id_product);
            return View(order_items);
        }

        // GET: admin/Order_items/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order_items order_items = db.Order_items.Find(id);
            if (order_items == null)
            {
                return HttpNotFound();
            }
            return View(order_items);
        }

        // POST: admin/Order_items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order_items order_items = db.Order_items.Find(id);
            db.Order_items.Remove(order_items);
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
