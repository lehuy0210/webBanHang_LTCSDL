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
    public class OrdersController : Controller
    {
        private webBHEntities db = new webBHEntities();

        // GET: admin/Orders
        public ActionResult Index()
        {
            var orders = db.Orders.Include(o => o.Delivery).Include(o => o.Payment).Include(o => o.User);
            return View(orders.ToList());
        }

        // GET: admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order_items = db.Order_items.Where(c => c.id_order == id).ToList();
            if (order_items == null)
            {
                return HttpNotFound();
            }
            return View(order_items);
        }

        // GET: admin/Orders/Create
        public ActionResult Create()
        {
            ViewBag.id_delivery = new SelectList(db.Deliverys, "id", "name");
            ViewBag.id_payment = new SelectList(db.Payments, "id", "name");
            ViewBag.id_user = new SelectList(db.Users, "id", "name");
            return View();
        }

        // POST: admin/Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,date,total_money,id_user,id_payment,id_delivery")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_delivery = new SelectList(db.Deliverys, "id", "name", order.id_delivery);
            ViewBag.id_payment = new SelectList(db.Payments, "id", "name", order.id_payment);
            ViewBag.id_user = new SelectList(db.Users, "id", "name", order.id_user);
            return View(order);
        }

        // GET: admin/Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_delivery = new SelectList(db.Deliverys, "id", "name", order.id_delivery);
            ViewBag.id_payment = new SelectList(db.Payments, "id", "name", order.id_payment);
            ViewBag.id_user = new SelectList(db.Users, "id", "name", order.id_user);
            return View(order);
        }

        // POST: admin/Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,date,total_money,id_user,id_payment,id_delivery")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_delivery = new SelectList(db.Deliverys, "id", "name", order.id_delivery);
            ViewBag.id_payment = new SelectList(db.Payments, "id", "name", order.id_payment);
            ViewBag.id_user = new SelectList(db.Users, "id", "name", order.id_user);
            return View(order);
        }

        // GET: admin/Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
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
