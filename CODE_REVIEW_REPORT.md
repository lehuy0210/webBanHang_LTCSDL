# 📋 BÁO CÁO REVIEW CODE - HỆ THỐNG BÁN HÀNG

**Ngày review:** 2025-11-12
**Reviewer:** Claude AI
**Project:** webBanHang_LTCSDL
**Framework:** ASP.NET MVC 5 + Entity Framework 6
**Database:** SQL Server

---

## 📧 VỊ TRÍ CHỨC NĂNG GỬI HÓA ĐƠN QUA GMAIL

### Vị trí code
**File:** `/webBH/webBH/Controllers/CartController.cs`
**Method:** `Order()`
**Dòng:** 169-175

### Code hiện tại
```csharp
MailMessage mail = new MailMessage("1951012033hoang@ou.edu.vn", users.email, "Thông tin đơn hàng", sMsg);
SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
mail.Body = sMsg;
mail.IsBodyHtml = true;
client.EnableSsl = true;
client.Credentials = new NetworkCredential("1951012033hoang@ou.edu.vn", "hh0817765357");
client.Send(mail);
```

### Luồng hoạt động
1. User nhấn nút "Đặt hàng" từ trang giỏ hàng (`/Cart/Order`)
2. Hệ thống tạo record Order trong database (dòng 113-120)
3. Tạo các Order_items từ giỏ hàng (dòng 134-145)
4. Build HTML table chứa thông tin sản phẩm (dòng 150-168)
5. **GỬI EMAIL** đến email của khách hàng qua Gmail SMTP
6. Xóa toàn bộ giỏ hàng (dòng 178-180)
7. Redirect về trang Cart với thông báo

---

## 🚨 VẤN ĐỀ BẢO MẬT NGHIÊM TRỌNG (CRITICAL)

### 1. HARDCODED PASSWORD - MỨC ĐỘ: 🔴 CRITICAL

**Vị trí:** `CartController.cs:174`

#### ❌ Vấn đề
```csharp
client.Credentials = new NetworkCredential("1951012033hoang@ou.edu.vn", "hh0817765357");
```

**Nguy cơ:**
- Mật khẩu email được lưu trực tiếp trong source code
- Code được push lên Git → mật khẩu public trên GitHub
- Bất kỳ ai clone repo đều thấy được password
- Tài khoản email có thể bị hack, dùng để spam, lừa đảo
- Vi phạm OWASP Top 10: A02:2021 – Cryptographic Failures

#### ✅ Giải pháp

**Bước 1:** Tạo App Password cho Gmail
1. Truy cập: https://myaccount.google.com/security
2. Bật 2-Step Verification
3. Vào "App passwords" → Tạo mật khẩu mới cho "Mail"
4. Copy mật khẩu 16 ký tự (dạng: xxxx-xxxx-xxxx-xxxx)

**Bước 2:** Lưu trong Web.config
```xml
<!-- File: Web.config -->
<appSettings>
  <add key="EmailFrom" value="1951012033hoang@ou.edu.vn" />
  <add key="EmailAppPassword" value="your-16-char-app-password" />
  <add key="SmtpHost" value="smtp.gmail.com" />
  <add key="SmtpPort" value="587" />
</appSettings>
```

**Bước 3:** Sửa code trong CartController.cs
```csharp
// Thêm using
using System.Configuration;

// Trong method Order(), thay thế dòng 169-175:
string emailFrom = ConfigurationManager.AppSettings["EmailFrom"];
string emailPassword = ConfigurationManager.AppSettings["EmailAppPassword"];
string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);

MailMessage mail = new MailMessage(emailFrom, users.email, "Thông tin đơn hàng", sMsg);
SmtpClient client = new SmtpClient(smtpHost, smtpPort);
mail.Body = sMsg;
mail.IsBodyHtml = true;
client.EnableSsl = true;
client.Credentials = new NetworkCredential(emailFrom, emailPassword);

try
{
    client.Send(mail);
    TempData["Message"] = "Đặt hàng thành công! Vui lòng kiểm tra email.";
}
catch (Exception ex)
{
    // Log error
    TempData["Message"] = "Đặt hàng thành công nhưng gửi email thất bại. Vui lòng liên hệ hotline.";
    // Log ex.Message vào file hoặc database
}
```

**Bước 4:** Thêm Web.config vào .gitignore
```gitignore
# Trong file .gitignore
Web.config
appsettings.json
*.config
```

**Bước 5:** Tạo Web.config.example
```xml
<!-- File: Web.config.example (mẫu cho developers khác) -->
<appSettings>
  <add key="EmailFrom" value="your-email@gmail.com" />
  <add key="EmailAppPassword" value="your-app-password-here" />
  <add key="SmtpHost" value="smtp.gmail.com" />
  <add key="SmtpPort" value="587" />
</appSettings>
```

---

### 2. THIẾU AUTHORIZATION - MỨC ĐỘ: 🔴 CRITICAL

**Vị trí:** Tất cả controllers trong `/Areas/admin/Controllers/`

#### ❌ Vấn đề
```csharp
public class ProductsController : Controller
{
    // Không có [Authorize] attribute
    // Bất kỳ ai biết URL đều truy cập được
}
```

**Nguy cơ:**
- Bất kỳ user nào (kể cả chưa đăng nhập) có thể:
  - Truy cập `/admin/Products` → xem danh sách sản phẩm
  - Truy cập `/admin/Products/Delete/5` → xóa sản phẩm
  - Truy cập `/admin/Orders` → xem tất cả đơn hàng
  - Truy cập `/admin/Users` → xem/sửa/xóa user
- Vi phạm OWASP Top 10: A01:2021 – Broken Access Control

#### ✅ Giải pháp

**Bước 1:** Tạo Custom Authorization Attribute
```csharp
// File: Models/CustomAuthorizeAttribute.cs
using System;
using System.Web;
using System.Web.Mvc;

namespace webBH.Models
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Kiểm tra đăng nhập
            if (httpContext.Session["UserId"] == null)
            {
                return false;
            }

            // Kiểm tra role nếu có
            if (!string.IsNullOrEmpty(Roles))
            {
                string userRole = httpContext.Session["UserRole"]?.ToString();
                if (userRole != Roles)
                {
                    return false;
                }
            }

            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Chưa đăng nhập → redirect đến login
            if (filterContext.HttpContext.Session["UserId"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Login" },
                        { "action", "Index" }
                    });
            }
            else
            {
                // Đăng nhập nhưng không đủ quyền → trang lỗi 403
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Unauthorized.cshtml"
                };
            }
        }
    }
}
```

**Bước 2:** Áp dụng cho tất cả Admin Controllers

```csharp
// File: Areas/admin/Controllers/ProductsController.cs
using webBH.Models;

namespace webBH.Areas.admin.Controllers
{
    [CustomAuthorize(Roles = "Admin")]  // Thêm dòng này
    public class ProductsController : Controller
    {
        // ... existing code
    }
}
```

**Áp dụng tương tự cho:**
- `OrdersController.cs`
- `CategoriesController.cs`
- `UsersController.cs`
- `PaymentsController.cs`
- `DeliveriesController.cs`
- `TransportsController.cs`
- `RolesController.cs`

**Bước 3:** Tạo View Unauthorized

```html
<!-- File: Views/Shared/Unauthorized.cshtml -->
@{
    ViewBag.Title = "Không có quyền truy cập";
}

<div class="container">
    <div class="row">
        <div class="col-md-12 text-center" style="margin-top: 100px;">
            <h1 style="font-size: 100px; color: #dc3545;">403</h1>
            <h2>Truy cập bị từ chối</h2>
            <p>Bạn không có quyền truy cập trang này.</p>
            <a href="@Url.Action("Index", "Home")" class="btn btn-primary">Về trang chủ</a>
        </div>
    </div>
</div>
```

---

### 3. SQL INJECTION POTENTIAL - MỨC ĐỘ: 🟠 HIGH

**Vị trí:**
- `HomeController.cs:22`
- `ProductsController.cs:25, 72`

#### ❌ Vấn đề
```csharp
var products = db.Products.Include(s => s.Category)
    .Where(x => x.name.ToUpper().Contains(SearchString.ToUpper()));
```

**Nguy cơ:**
- Dù EF tự động parameterize, nhưng không validate input
- User có thể nhập ký tự đặc biệt: `'; DROP TABLE Products; --`
- Có thể bypass search với các ký tự wildcard

#### ✅ Giải pháp

```csharp
// File: HomeController.cs
public ActionResult Index(string currentFilter, int? page, int id_category = 0, string SearchString = "")
{
    // Validate và sanitize input
    if (!string.IsNullOrWhiteSpace(SearchString))
    {
        // Chỉ cho phép chữ, số, khoảng trắng
        if (!System.Text.RegularExpressions.Regex.IsMatch(SearchString, @"^[a-zA-Z0-9\s\u00C0-\u1EF9]+$"))
        {
            ModelState.AddModelError("", "Từ khóa tìm kiếm không hợp lệ");
            return View(db.Products.Include(s => s.Category).ToList());
        }

        // Giới hạn độ dài
        if (SearchString.Length > 100)
        {
            SearchString = SearchString.Substring(0, 100);
        }

        page = 1;
        SearchString = SearchString.Trim();
        var products = db.Products.Include(s => s.Category)
            .Where(x => x.name.ToUpper().Contains(SearchString.ToUpper()));
        return View(products.ToList());
    }
    else
    {
        var products = db.Products.Include(s => s.Category);
        return View(products.ToList());
    }
}
```

---

### 4. FILE UPLOAD VULNERABILITY - MỨC ĐỘ: 🔴 CRITICAL

**Vị trí:** `Areas/admin/Controllers/ProductsController.cs:56-66, 97-111`

#### ❌ Vấn đề
```csharp
if(image != null && image.ContentLength > 0)
{
    string filename = Path.GetFileName(image.FileName);
    string path = Server.MapPath("~/Images/" + filename);
    product.image = "Images/" + filename;
    image.SaveAs(path);
}
```

**Nguy cơ:**
- Upload file `.exe`, `.aspx`, `.config` → Remote Code Execution
- Upload file khổng lồ → DoS attack
- Path Traversal: filename = `../../Web.config` → ghi đè file hệ thống
- Dùng tên file gốc → xung đột tên, ghi đè file
- Vi phạm OWASP Top 10: A04:2021 – Insecure Design

#### ✅ Giải pháp

**Bước 1:** Tạo Helper cho Upload

```csharp
// File: Models/FileUploadHelper.cs
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace webBH.Models
{
    public static class FileUploadHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
        private const int MaxFileSize = 5 * 1024 * 1024; // 5MB

        public static (bool success, string message, string filePath) SaveImage(HttpPostedFileBase file, string uploadDirectory)
        {
            // Kiểm tra file null
            if (file == null || file.ContentLength == 0)
            {
                return (false, "Vui lòng chọn file", null);
            }

            // Kiểm tra kích thước
            if (file.ContentLength > MaxFileSize)
            {
                return (false, "File không được vượt quá 5MB", null);
            }

            // Kiểm tra MIME type
            if (!AllowedMimeTypes.Contains(file.ContentType.ToLower()))
            {
                return (false, "Chỉ chấp nhận file ảnh (JPG, PNG, GIF, WebP)", null);
            }

            // Kiểm tra extension
            string extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
            {
                return (false, "Phần mở rộng file không hợp lệ", null);
            }

            // Tạo tên file mới (unique)
            string newFileName = $"{Guid.NewGuid()}{extension}";
            string physicalPath = Path.Combine(uploadDirectory, newFileName);
            string relativePath = $"Images/{newFileName}";

            try
            {
                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                // Lưu file
                file.SaveAs(physicalPath);

                return (true, "Upload thành công", relativePath);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi lưu file: {ex.Message}", null);
            }
        }

        public static void DeleteImage(string serverPath, string relativePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(relativePath))
                {
                    string physicalPath = Path.Combine(serverPath, relativePath.Replace("Images/", ""));
                    if (File.Exists(physicalPath))
                    {
                        File.Delete(physicalPath);
                    }
                }
            }
            catch
            {
                // Log error nhưng không throw để không ảnh hưởng flow chính
            }
        }
    }
}
```

**Bước 2:** Sửa ProductsController

```csharp
// File: Areas/admin/Controllers/ProductsController.cs

[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Create([Bind(Include = "id,name,price,size,color,id_category")] Product product, HttpPostedFileBase image)
{
    if (ModelState.IsValid)
    {
        // Upload ảnh
        if (image != null)
        {
            string uploadDir = Server.MapPath("~/Images");
            var result = FileUploadHelper.SaveImage(image, uploadDir);

            if (!result.success)
            {
                ModelState.AddModelError("image", result.message);
                ViewBag.id_category = new SelectList(db.Categories, "id", "name", product.id_category);
                return View(product);
            }

            product.image = result.filePath;
        }
        else
        {
            // Ảnh mặc định nếu không upload
            product.image = "Images/no-image.png";
        }

        db.Products.Add(product);
        db.SaveChanges();
        return RedirectToAction("Index");
    }

    ViewBag.id_category = new SelectList(db.Categories, "id", "name", product.id_category);
    return View(product);
}

[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Edit([Bind(Include = "id,name,price,size,color,id_category")] Product product, HttpPostedFileBase imageUpload, string imageSP)
{
    if (ModelState.IsValid)
    {
        if (imageUpload != null)
        {
            // Xóa ảnh cũ nếu có
            if (!string.IsNullOrEmpty(imageSP))
            {
                FileUploadHelper.DeleteImage(Server.MapPath("~/Images"), imageSP);
            }

            // Upload ảnh mới
            string uploadDir = Server.MapPath("~/Images");
            var result = FileUploadHelper.SaveImage(imageUpload, uploadDir);

            if (!result.success)
            {
                ModelState.AddModelError("imageUpload", result.message);
                ViewBag.id_category = new SelectList(db.Categories, "id", "name", product.id_category);
                return View(product);
            }

            product.image = result.filePath;
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
```

---

### 5. SESSION FIXATION & HIJACKING - MỨC ĐỘ: 🟠 HIGH

**Vị trí:** `LoginController.cs:52-56`

#### ❌ Vấn đề
```csharp
Session["UserId"] = find_user.id;
Session["UserEmail"] = find_user.email;
```

**Nguy cơ:**
- Session Fixation: attacker có thể cố định session ID trước khi login
- Session không được regenerate sau khi đăng nhập
- Không có secure cookie flags
- Session có thể bị đánh cắp qua XSS

#### ✅ Giải pháp

```csharp
// File: LoginController.cs

[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Index([Bind(Include = "email,password")] User user)
{
    var find_user = db.Users.FirstOrDefault(u => u.email == user.email);

    if (find_user != null)
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
                // REGENERATE SESSION để tránh session fixation
                Session.Clear();
                Session.Abandon();

                // Tạo session mới
                Session["UserId"] = find_user.id;
                Session["UserEmail"] = find_user.email;
                Session["UserName"] = find_user.name;
                Session["UserRole"] = find_user.Role.name;
                Session["RoleId"] = find_user.id_roles;

                // Set timeout (30 phút)
                Session.Timeout = 30;

                // Log activity
                LogUserActivity(find_user.id, "Login", HttpContext.Request.UserHostAddress);

                return RedirectToAction("Index", "Home");
            }
        }
    }

    // Chống brute force: log failed attempts
    LogFailedLogin(user.email, HttpContext.Request.UserHostAddress);

    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
    return View();
}

private void LogUserActivity(int userId, string action, string ipAddress)
{
    // TODO: Lưu vào database hoặc file log
    // Table: UserActivityLog (id, user_id, action, ip_address, timestamp)
}

private void LogFailedLogin(string email, string ipAddress)
{
    // TODO: Log failed login attempts
    // Implement rate limiting sau X lần thất bại
}
```

**Bước 3:** Thêm vào Web.config

```xml
<system.web>
  <sessionState
    mode="InProc"
    timeout="30"
    cookieless="UseCookies"
    cookieName=".ASPXSESSION"
    regenerateExpiredSessionId="true">
  </sessionState>

  <httpCookies
    httpOnlyCookies="true"
    requireSSL="true"
    sameSite="Strict" />
</system.web>
```

---

## ⚠️ VẤN ĐỀ CHẤT LƯỢNG CODE (CODE QUALITY)

### 6. CODE DUPLICATION - MỨC ĐỘ: 🟡 MEDIUM

#### ❌ Vấn đề
Đoạn code kiểm tra session được lặp lại ở:
- `HomeController.cs:45-61`
- `ProductsController.cs:48-64`
- `ProductsController.cs:95-111`

```csharp
// Lặp lại ở nhiều nơi
if (Session["UserId"] != null && Session["UserEmail"] != null)
{
    int userId = Convert.ToInt32(Session["UserId"]);
    string username = Session["UserEmail"].ToString();
    User user = db.Users.Find(userId);
    return View(products.ToList());
}
else
{
    return RedirectToAction("index", "login");
}
```

#### ✅ Giải pháp

**Tạo Base Controller**

```csharp
// File: Controllers/BaseController.cs
using System;
using System.Web.Mvc;
using webBH.Models;

namespace webBH.Controllers
{
    public class BaseController : Controller
    {
        protected webBHEntities db = new webBHEntities();

        // Property để lấy User hiện tại
        protected User CurrentUser
        {
            get
            {
                if (Session["UserId"] != null)
                {
                    int userId = Convert.ToInt32(Session["UserId"]);
                    return db.Users.Find(userId);
                }
                return null;
            }
        }

        // Property để lấy UserId
        protected int? CurrentUserId
        {
            get
            {
                if (Session["UserId"] != null)
                {
                    return Convert.ToInt32(Session["UserId"]);
                }
                return null;
            }
        }

        // Check đăng nhập
        protected bool IsAuthenticated
        {
            get
            {
                return Session["UserId"] != null && Session["UserEmail"] != null;
            }
        }

        // Check quyền admin
        protected bool IsAdmin
        {
            get
            {
                return Session["UserRole"]?.ToString() == "Admin";
            }
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
```

**Sử dụng BaseController**

```csharp
// File: Controllers/HomeController.cs
using System.Web.Mvc;
using System.Data.Entity;

namespace webBH.Controllers
{
    public class HomeController : BaseController  // Thay đổi: kế thừa từ BaseController
    {
        public ActionResult Index(string currentFilter, int? page, int id_category = 0, string SearchString = "")
        {
            // Check đăng nhập đơn giản
            if (!IsAuthenticated)
            {
                return RedirectToAction("Index", "Login");
            }

            // Xử lý search
            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                page = 1;
                var products = db.Products.Include(s => s.Category)
                    .Where(x => x.name.ToUpper().Contains(SearchString.ToUpper()));
                return View(products.ToList());
            }

            var allProducts = db.Products.Include(s => s.Category);
            return View(allProducts.ToList());
        }
    }
}
```

**Áp dụng cho các controllers khác:**
- `CartController`
- `ProductsController`
- `LogoutController`

---

### 7. MAGIC NUMBERS - MỨC ĐỘ: 🟡 MEDIUM

#### ❌ Vấn đề
```csharp
order.id_payment = 1;   // 1 là gì?
order.id_delivery = 1;  // 1 là gì?
user.id_roles = 1;      // 1 là gì?
```

#### ✅ Giải pháp

```csharp
// File: Models/Constants.cs
namespace webBH.Models
{
    public static class PaymentMethods
    {
        public const int CashOnDelivery = 1;
        public const int BankTransfer = 2;
        public const int CreditCard = 3;
    }

    public static class DeliveryMethods
    {
        public const int Standard = 1;
        public const int Express = 2;
        public const int SameDay = 3;
    }

    public static class UserRoles
    {
        public const int User = 1;
        public const int Admin = 2;
        public const int Moderator = 3;
    }

    public static class ProductCategories
    {
        public const int Ao = 1;      // Áo
        public const int Quan = 2;    // Quần
    }
}
```

**Sử dụng:**

```csharp
// File: Controllers/CartController.cs
using webBH.Models;

Order order = new Order();
order.date = now;
order.total_money = total;
order.id_user = userId;
order.id_payment = PaymentMethods.CashOnDelivery;  // Rõ ràng hơn
order.id_delivery = DeliveryMethods.Standard;      // Rõ ràng hơn

// File: Controllers/RegisterController.cs
user.id_roles = UserRoles.User;  // Rõ ràng hơn
```

---

### 8. TYPO: "QUANLITY" → "QUANTITY" - MỨC ĐỘ: 🟡 MEDIUM

#### ❌ Vấn đề
```csharp
// File: CartController.cs:138
var order_items = new Order_items
{
    quanlity = cart.quantity,  // Sai chính tả
    id_order = latestRow.id,
    id_product = (int)cart.id_product,
    total_money = cart.total_money,
};
```

#### ✅ Giải pháp

**Cách 1: Sửa trong database (khuyến nghị)**

```sql
-- Chạy SQL script
USE qlbanhang;

-- Rename column
EXEC sp_rename 'Order_items.quanlity', 'quantity', 'COLUMN';
```

**Sau đó update Entity Framework Model:**
1. Mở file `BanHangEF.edmx`
2. Right click → "Update Model from Database"
3. Chọn table `Order_items` → Finish

**Cách 2: Nếu không thể sửa database, dùng Column attribute**

```csharp
// File: Models/Order_items.cs (partial class)
using System.ComponentModel.DataAnnotations.Schema;

namespace webBH.Models
{
    [MetadataType(typeof(Order_itemsMetadata))]
    public partial class Order_items
    {
    }

    public class Order_itemsMetadata
    {
        [Column("quanlity")]  // Map tên column đúng trong DB
        public int? quantity { get; set; }  // Property name đúng
    }
}
```

---

### 9. COMMENTED CODE KHÔNG XÓA - MỨC ĐỘ: 🟢 LOW

#### ❌ Vấn đề
- `HomeController.cs:32-44` có code bị comment
- `CartController.cs:122-133` có code bị comment
- `ProductsController.cs:36-47, 82-94` có code bị comment

#### ✅ Giải pháp

**Xóa tất cả commented code**

Nếu cần lưu lại code cũ → dùng Git history, không nên comment trong source code.

```csharp
// XÓA tất cả đoạn này
//if(id_category == 0)
//{
//    int pageSize = 8;
//    int pageNumber = (page ?? 1);
//    var products = db.Products.Include(s => s.Category).OrderBy(x => x.name);
//    return View(products.ToPagedList(pageNumber, pageSize));
//}
```

---

### 10. MISSING ERROR HANDLING - MỨC ĐỘ: 🟠 HIGH

#### ❌ Vấn đề
- Không có try-catch cho gửi email
- Không có global error handling
- Không log exceptions

#### ✅ Giải pháp

**Bước 1: Tạo Error Logger**

```csharp
// File: Models/ErrorLogger.cs
using System;
using System.IO;
using System.Web;

namespace webBH.Models
{
    public static class ErrorLogger
    {
        private static readonly string LogPath = HttpContext.Current.Server.MapPath("~/App_Data/Logs");

        public static void Log(Exception ex, string additionalInfo = "")
        {
            try
            {
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }

                string fileName = $"Error_{DateTime.Now:yyyyMMdd}.log";
                string filePath = Path.Combine(LogPath, fileName);

                string logEntry = $@"
==================== ERROR LOG ====================
Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Additional Info: {additionalInfo}
Message: {ex.Message}
Stack Trace: {ex.StackTrace}
Inner Exception: {ex.InnerException?.Message}
===================================================
";

                File.AppendAllText(filePath, logEntry);
            }
            catch
            {
                // Không throw error trong logger
            }
        }

        public static void LogInfo(string message)
        {
            try
            {
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }

                string fileName = $"Info_{DateTime.Now:yyyyMMdd}.log";
                string filePath = Path.Combine(LogPath, fileName);

                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";

                File.AppendAllText(filePath, logEntry);
            }
            catch
            {
                // Không throw error trong logger
            }
        }
    }
}
```

**Bước 2: Áp dụng Error Handling**

```csharp
// File: Controllers/CartController.cs

public ActionResult Order()
{
    try
    {
        DateTime now = DateTime.Now;
        int userId = Convert.ToInt32(HttpContext.Session["UserId"]);
        List<Cart> cartsList = db.Carts.Where(c => c.id_user == userId).ToList();

        if (cartsList.Count == 0)
        {
            TempData["Message"] = "Giỏ hàng trống";
            return RedirectToAction("Index", "Cart");
        }

        // Tính tổng tiền
        int? total = cartsList.Sum(item => item.total_money);

        // Tạo đơn hàng
        Order order = new Order
        {
            date = now,
            total_money = total,
            id_user = userId,
            id_payment = PaymentMethods.CashOnDelivery,
            id_delivery = DeliveryMethods.Standard
        };
        db.Orders.Add(order);
        db.SaveChanges();

        // Lấy đơn hàng vừa tạo
        var latestOrder = db.Orders
            .Where(c => c.id_user == userId)
            .OrderByDescending(x => x.id)
            .FirstOrDefault();

        // Tạo order items
        foreach (var cart in cartsList)
        {
            var orderItem = new Order_items
            {
                quanlity = cart.quantity,
                id_order = latestOrder.id,
                id_product = (int)cart.id_product,
                total_money = cart.total_money,
            };
            db.Order_items.Add(orderItem);
        }
        db.SaveChanges();

        // Gửi email
        bool emailSent = SendOrderEmail(cartsList, total, userId);

        // Xóa giỏ hàng
        var items = db.Carts.Where(x => x.id_user == userId);
        db.Carts.RemoveRange(items);
        db.SaveChanges();

        if (emailSent)
        {
            TempData["Message"] = "Đặt hàng thành công! Vui lòng kiểm tra email.";
        }
        else
        {
            TempData["Message"] = "Đặt hàng thành công! (Email gửi thất bại, vui lòng liên hệ hotline)";
        }

        return RedirectToAction("Index", "Cart");
    }
    catch (Exception ex)
    {
        ErrorLogger.Log(ex, $"Order failed for user: {HttpContext.Session["UserId"]}");
        TempData["Message"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.";
        return RedirectToAction("Index", "Cart");
    }
}

private bool SendOrderEmail(List<Cart> cartsList, int? total, int userId)
{
    try
    {
        string emailFrom = ConfigurationManager.AppSettings["EmailFrom"];
        string emailPassword = ConfigurationManager.AppSettings["EmailAppPassword"];
        string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
        int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);

        var user = db.Users.Find(userId);
        if (user == null || string.IsNullOrEmpty(user.email))
        {
            return false;
        }

        // Build HTML
        string sMsg = "<html><body><table border='1'><caption>Thông tin đặt hàng</caption>";
        sMsg += "<tr><th>STT</th><th>Tên sản phẩm</th><th>Số lượng</th><th>Thành tiền</th></tr>";

        int i = 0;
        foreach (Cart item in cartsList)
        {
            var product = db.Products.FirstOrDefault(x => x.id == item.id_product);
            if (product != null)
            {
                i++;
                sMsg += $"<tr><td>{i}</td><td>{product.name}</td><td>{item.quantity}</td><td>{item.total_money:N0} đ</td></tr>";
            }
        }

        sMsg += $"<tr><th colspan='4'>Tổng cộng: {total:N0} đ</th></tr></table></body></html>";

        // Gửi email
        MailMessage mail = new MailMessage(emailFrom, user.email, "Thông tin đơn hàng", sMsg);
        SmtpClient client = new SmtpClient(smtpHost, smtpPort);
        mail.Body = sMsg;
        mail.IsBodyHtml = true;
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential(emailFrom, emailPassword);
        client.Send(mail);

        ErrorLogger.LogInfo($"Email sent successfully to {user.email} for order total: {total}");
        return true;
    }
    catch (Exception ex)
    {
        ErrorLogger.Log(ex, $"Failed to send email for user: {userId}");
        return false;
    }
}
```

**Bước 3: Global Error Handler**

```csharp
// File: Global.asax.cs
using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using webBH.Models;

namespace webBH
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            Exception ex = Server.GetLastError();
            ErrorLogger.Log(ex, "Unhandled Application Error");

            // Optional: Redirect to error page
            // Response.Redirect("~/Error");
        }
    }
}
```

---

### 11. NO INPUT VALIDATION - MỨC ĐỘ: 🟠 HIGH

#### ❌ Vấn đề
- Không validate email format
- Không validate password strength
- Không validate số điện thoại
- Không validate ngày sinh

#### ✅ Giải pháp

**Thêm Data Annotations vào Model**

```csharp
// File: Models/User.Validation.cs (partial class)
using System;
using System.ComponentModel.DataAnnotations;

namespace webBH.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
    }

    public class UserMetadata
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Họ tên chỉ chứa chữ cái")]
        public string name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
        public string email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ hoa, 1 chữ thường và 1 số")]
        public string password { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> birthday { get; set; }
    }
}
```

**Thêm Custom Validation cho Birthday**

```csharp
// File: Models/CustomValidations.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace webBH.Models
{
    public class AgeRangeAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        private readonly int _maxAge;

        public AgeRangeAttribute(int minAge, int maxAge)
        {
            _minAge = minAge;
            _maxAge = maxAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            DateTime birthday = (DateTime)value;
            int age = DateTime.Now.Year - birthday.Year;

            if (birthday > DateTime.Now.AddYears(-age))
                age--;

            if (age < _minAge || age > _maxAge)
            {
                return new ValidationResult($"Tuổi phải từ {_minAge} đến {_maxAge}");
            }

            return ValidationResult.Success;
        }
    }
}
```

**Sử dụng:**

```csharp
// File: Models/User.Validation.cs
[AgeRange(13, 120, ErrorMessage = "Tuổi phải từ 13 đến 120")]
public Nullable<System.DateTime> birthday { get; set; }
```

---

### 12. NO DEPENDENCY INJECTION - MỨC ĐỘ: 🟡 MEDIUM

#### ❌ Vấn đề
```csharp
private webBHEntities db = new webBHEntities();
```

DbContext được tạo trực tiếp → khó unit test, tight coupling

#### ✅ Giải pháp (Optional - cho project lớn)

```csharp
// File: Models/IRepository.cs
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace webBH.Models
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        void SaveChanges();
    }
}

// File: Models/Repository.cs
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace webBH.Models
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly webBHEntities _context;
        private readonly DbSet<T> _dbSet;

        public Repository(webBHEntities context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void Update(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            T entity = _dbSet.Find(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
```

---

## 📋 CHECKLIST TRIỂN KHAI

### Priority 1: KHẨN CẤP (Làm ngay)
- [ ] **Đổi hardcoded password** → App Password + Web.config
- [ ] **Thêm [Authorize] cho admin area** → Tất cả admin controllers
- [ ] **Fix file upload vulnerability** → Validate extension, size, MIME type
- [ ] **Add try-catch cho email sending** → Error handling
- [ ] **Tạo Web.config.example** và thêm Web.config vào .gitignore

### Priority 2: QUAN TRỌNG (Tuần này)
- [ ] Fix session management → Regenerate session sau login
- [ ] Add input validation → Email, password, birthday
- [ ] Add SQL injection protection → Validate search input
- [ ] Create ErrorLogger → Log tất cả exceptions
- [ ] Add global error handler → Application_Error()

### Priority 3: CẢI THIỆN (Tuần sau)
- [ ] Refactor code duplication → Tạo BaseController
- [ ] Replace magic numbers → Tạo Constants class
- [ ] Fix typo "quanlity" → "quantity"
- [ ] Xóa commented code → Clean up
- [ ] Add logging cho user activities

### Priority 4: TỐI ƯU (Tương lai)
- [ ] Implement Repository Pattern (optional)
- [ ] Add caching cho products
- [ ] Implement rate limiting cho login
- [ ] Add CAPTCHA cho register/login
- [ ] Implement email queue (không gửi trực tiếp)

---

## 🔧 CÔNG CỤ HỖ TRỢ

### Extensions nên cài cho Visual Studio:
1. **SonarLint** - Phát hiện security issues
2. **ReSharper** - Code quality analysis
3. **CodeMaid** - Clean up code
4. **Web Essentials** - CSS/JS optimization

### Tools kiểm tra security:
1. **OWASP ZAP** - Penetration testing
2. **Fiddler** - Debug HTTP traffic
3. **SQL Injection Scanner** - Test SQL injection

---

## 📚 TÀI LIỆU THAM KHẢO

1. **OWASP Top 10 2021:** https://owasp.org/Top10/
2. **ASP.NET Security Best Practices:** https://learn.microsoft.com/en-us/aspnet/mvc/overview/security/
3. **Entity Framework Security:** https://learn.microsoft.com/en-us/ef/
4. **Gmail SMTP App Passwords:** https://support.google.com/accounts/answer/185833

---

## 📞 LIÊN HỆ & HỖ TRỢ

Nếu cần hỗ trợ thêm trong quá trình triển khai, vui lòng:
1. Tạo issue trên GitHub repository
2. Ghi rõ vấn đề gặp phải
3. Attach error logs nếu có

---

**Lưu ý:** Đây là báo cáo review code chi tiết. Nên triển khai từng bước theo thứ tự ưu tiên để đảm bảo hệ thống vẫn hoạt động ổn định.

**Khuyến nghị:** Backup database và source code trước khi bắt đầu sửa!
