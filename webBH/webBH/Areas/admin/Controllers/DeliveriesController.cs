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
    public class DeliveriesController : Controller
    {
        private webBHEntities db = new webBHEntities();

        // GET: admin/Deliveries
        public ActionResult Index()
        {
            var deliverys = db.Deliverys.Include(d => d.Transport);
            return View(deliverys.ToList());
        }

        // GET: admin/Deliveries/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Delivery delivery = db.Deliverys.Find(id);
            if (delivery == null)
            {
                return HttpNotFound();
            }
            return View(delivery);
        }

        // GET: admin/Deliveries/Create
        public ActionResult Create()
        {
            ViewBag.id_transport = new SelectList(db.Transports, "id", "name");
            return View();
        }

        // POST: admin/Deliveries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,date,id_transport")] Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                db.Deliverys.Add(delivery);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_transport = new SelectList(db.Transports, "id", "name", delivery.id_transport);
            return View(delivery);
        }

        // GET: admin/Deliveries/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Delivery delivery = db.Deliverys.Find(id);
            if (delivery == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_transport = new SelectList(db.Transports, "id", "name", delivery.id_transport);
            return View(delivery);
        }

        // POST: admin/Deliveries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,date,id_transport")] Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                db.Entry(delivery).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_transport = new SelectList(db.Transports, "id", "name", delivery.id_transport);
            return View(delivery);
        }

        // GET: admin/Deliveries/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Delivery delivery = db.Deliverys.Find(id);
            if (delivery == null)
            {
                return HttpNotFound();
            }
            return View(delivery);
        }

        // POST: admin/Deliveries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Delivery delivery = db.Deliverys.Find(id);
            db.Deliverys.Remove(delivery);
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
