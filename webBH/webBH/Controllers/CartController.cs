using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using webBH.Models;
using System.Net.Mail;
namespace webBH.Controllers
{
    public class CartController : Controller
    {
        private webBHEntities db = new webBHEntities();

        // GET: Cart
        public ActionResult Index()
        {
            
            int userId = Convert.ToInt32(HttpContext.Session["UserId"]);
            var cart = db.Carts.Where(c => c.id_user == userId).ToList();
            List<Cart> cartsList = db.Carts.Where(c => c.id_user == userId).ToList();
            int? total = 0;
            foreach(Cart item in cartsList)
            {
                total = total + item.total_money;
            }
            ViewBag.total = total;
            return View(cart);
        }
        public ActionResult AddToCart(int? id_product, int id_user)
        {

            var cart = db.Carts.Where(c => c.id_user == id_user && c.id_product == id_product).FirstOrDefault();
            if(cart == null)
            {
                Cart cart1 = new Cart();
                cart1.id_product = id_product;
                var product = db.Products.Find(id_product);
                cart1.id_user = id_user;
                cart1.quantity = 1;
                cart1.total_money = cart1.quantity * (int?)product.price; 
                db.Carts.Add(cart1);
                db.SaveChanges();
            }
            else
            {
                cart.id_product = id_product;
                var product = db.Products.Find(id_product);
                cart.id_user = id_user;
                cart.quantity = cart.quantity +1;
                cart.total_money = cart.quantity * (int?)product.price;
                db.Entry(cart).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Delete(int? id)               
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cart cart = db.Carts.Find(id);
            if (cart == null)
            {
                return HttpNotFound();
            }
            return View(cart);
        }

        // POST: admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Cart cart = db.Carts.Find(id);
            db.Carts.Remove(cart);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Update(int? id, int? quantity, int? id_product)
        {
            var cart = db.Carts.Find(id);
            var product = db.Products.Find(id_product);
            if(cart != null)
            {
                cart.quantity = quantity;
                cart.total_money = cart.quantity * (int?)product.price;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
                //db.Entry(cart).State = EntityState.Modified;
                //db.SaveChanges();  
            return View();
        }
        public ActionResult Order()
        {
            DateTime now = DateTime.Now;
            int userId = Convert.ToInt32(HttpContext.Session["UserId"]);
            List<Cart> cartsList = db.Carts.Where(c => c.id_user == userId).ToList();

            if(cartsList.Count != 0)
            {
                int? total = 0;


                foreach (Cart item in cartsList)
                {
                    total = total + item.total_money;
                }
                Order order = new Order();
                order.date = now;
                order.total_money = total;
                order.id_user = userId;
                order.id_payment = 1;
                order.id_delivery = 1;
                db.Orders.Add(order);
                db.SaveChanges();
                var latestRow = db.Orders.Where(c => c.id_user == userId).OrderByDescending(x => x.id).FirstOrDefault();
                //var order_items = cartsList.Select(x => new Order_items
                //{
                //    // set các thuộc tính của đối tượng Table2 tương ứng với đối tượng Table1
                //    quanlity = x.quantity,
                //    id_order = latestRow.id,
                //    id_product = (int)x.id_product,
                //    total_money = x.total_money,

                //    // ...
                //}).ToList();
                //db.Order_items.AddRange(order_items);
                //db.SaveChanges();
                foreach (var cart in cartsList)
                {
                    var order_items = new Order_items
                    {
                        quanlity = cart.quantity,
                        id_order = latestRow.id,
                        id_product = (int)cart.id_product,
                        total_money = cart.total_money,
                    };
                    db.Order_items.Add(order_items);
                }
                db.SaveChanges();
                int id_cart = cartsList.FirstOrDefault().id;



                string sMsg = "<html><body><table border='1'><caption>Thông tin đặt hàng</caption><tr><th> STT </th> <th>Tên sản phẩm </th><th> Số lượng </th> <th> Thành tiền </th></tr> ";
                int i = 0;
                foreach (Cart item in cartsList)
                {
                    var product = db.Products.Where(x => x.id == item.id_product).FirstOrDefault();

                    Console.WriteLine(item);
                    i++;
                    sMsg += "<tr>";
                    sMsg += "<td>" + i.ToString() + "</td>";
                    sMsg += "<td>" + product.name + "</td>";
                    sMsg += "<td>" + item.quantity.ToString() + "</td>";
                    sMsg += "<td>" + item.total_money + "đ" + "</td>";
                    sMsg += "</tr>";

                }

                var users = db.Users.Find(userId);
                sMsg += "<tr><th colspan='5'> Tổng cộng: " + total + " đ" + "</th> </table> </body></html>";
                MailMessage mail = new MailMessage("1951012033hoang@ou.edu.vn", users.email, "Thông tin đơn hàng", sMsg);
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                mail.Body = sMsg;
                mail.IsBodyHtml = true;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential("1951012033hoang@ou.edu.vn", "hh0817765357");
                client.Send(mail);
                TempData["Message"] = "Please check the invoice sent to your email.";

                var items = db.Carts.Where(x => x.id_user == userId);
                db.Carts.RemoveRange(items);
                db.SaveChanges();



                return RedirectToAction("Index", "Cart");
            }
            else
            {
                TempData["Message"] = "Don't have product in your cart";
                return RedirectToAction("Index", "Cart");
            }


        }

    }
}