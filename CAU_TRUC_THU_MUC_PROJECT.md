# 📁 CẤU TRÚC THỨ MỤC PROJECT - HƯỚNG DẪN CHI TIẾT

## 🏗️ TỔNG QUAN KIẾN TRÚC ASP.NET MVC

Project này sử dụng **ASP.NET MVC 5** với **Entity Framework 6**, tuân theo mô hình **MVC** (Model-View-Controller):

```
webBanHang_LTCSDL/
├── Nhom17_baocao.docx          ← Báo cáo đồ án
├── qlbanhang.sql               ← Database SQL script
└── webBH/                      ← Solution folder
    └── webBH/                  ← Main project folder ⭐
        ├── App_Data/           ← Dữ liệu ứng dụng (database files, logs)
        ├── App_Start/          ← Cấu hình khởi động ứng dụng
        ├── Areas/              ← Khu vực riêng (Admin panel)
        ├── bin/                ← File .dll compiled
        ├── Content/            ← CSS, images, fonts
        ├── Controllers/        ← Controllers (xử lý logic)
        ├── fonts/              ← Font files
        ├── Images/             ← Hình ảnh sản phẩm
        ├── Models/             ← Models (dữ liệu, Entity Framework)
        ├── obj/                ← Object files (compile temporary)
        ├── Properties/         ← Assembly info
        ├── Scripts/            ← JavaScript/jQuery files
        ├── Views/              ← Views (giao diện HTML)
        ├── Global.asax         ← Application startup
        ├── Web.config          ← Cấu hình chính
        ├── packages.config     ← NuGet packages
        └── webBH.csproj        ← Project file
```

---

## 📂 CHI TIẾT TỪNG THỨ MỤC

---

### 1️⃣ **App_Data/** - Thư mục dữ liệu

**Chức năng:** Lưu trữ dữ liệu ứng dụng (database files, logs, XML, JSON...)

**Đặc điểm:**
- Không thể truy cập trực tiếp từ trình duyệt (bảo mật)
- Thường chứa database `.mdf` (SQL Server LocalDB)
- Có thể chứa file logs, cache

**Trong project này:**
```
App_Data/
└── (Trống - database đang dùng SQL Server remote)
```

**Khi nào cần sửa:**
- Nếu dùng LocalDB → đặt file `.mdf` vào đây
- Nếu muốn lưu logs → tạo thư mục `Logs/` trong này

**Ví dụ sử dụng:**
```csharp
// Đường dẫn đến App_Data
string path = Server.MapPath("~/App_Data/myfile.txt");
```

---

### 2️⃣ **App_Start/** - Cấu hình khởi động ⭐ QUAN TRỌNG

**Chức năng:** Chứa các file cấu hình được chạy khi ứng dụng khởi động

**Các file trong thư mục:**

```
App_Start/
├── BundleConfig.cs           ← Cấu hình CSS/JS bundling ⭐
├── FilterConfig.cs           ← Cấu hình filters (error handling)
├── IdentityConfig.cs         ← Cấu hình ASP.NET Identity
├── RouteConfig.cs            ← Cấu hình routing/URL ⭐
└── Startup.Auth.cs           ← Cấu hình authentication
```

#### 📄 **BundleConfig.cs** - Quản lý CSS/JS

**Chức năng:** Gom nhiều file CSS/JS thành 1 file (giảm HTTP requests)

```csharp
// File: App_Start/BundleConfig.cs

public static void RegisterBundles(BundleCollection bundles)
{
    // Bundle jQuery
    bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
        "~/Scripts/jquery-{version}.js"));

    // Bundle Bootstrap
    bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
        "~/Scripts/bootstrap.js"));

    // Bundle CSS
    bundles.Add(new StyleBundle("~/Content/css").Include(
        "~/Content/bootstrap.css",
        "~/Content/site.css"));
}
```

**Khi nào cần sửa:**
- Thêm CSS/JS mới vào bundle
- Tạo bundle mới cho trang cụ thể
- Tối ưu performance

**Ví dụ thêm file mới:**
```csharp
bundles.Add(new StyleBundle("~/Content/css").Include(
    "~/Content/bootstrap.css",
    "~/Content/site.css",
    "~/Content/custom.css"  // ← Thêm file mới
));
```

---

#### 📄 **RouteConfig.cs** - Định nghĩa URL

**Chức năng:** Cấu hình URL routing (URL đẹp, SEO-friendly)

```csharp
// File: App_Start/RouteConfig.cs

public static void RegisterRoutes(RouteCollection routes)
{
    routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

    routes.MapRoute(
        name: "Default",
        url: "{controller}/{action}/{id}",
        defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
    );
}
```

**Giải thích:**
- `{controller}` → Tên controller (Home, Cart, Products...)
- `{action}` → Tên action method (Index, Detail, Delete...)
- `{id}` → Parameter tùy chọn

**Ví dụ URL mapping:**
```
/Home/Index           → HomeController.Index()
/Products/Detail/5    → ProductsController.Detail(5)
/Cart/Delete/3        → CartController.Delete(3)
```

**Khi nào cần sửa:**
- Tạo custom URL (ví dụ: `/san-pham/ao-thun` thay vì `/Products/Detail/5`)
- Thêm area mới
- SEO optimization

---

#### 📄 **FilterConfig.cs** - Error handling

**Chức năng:** Đăng ký global filters (xử lý lỗi, authorize...)

```csharp
public static void RegisterGlobalFilters(GlobalFilterCollection filters)
{
    filters.Add(new HandleErrorAttribute());
}
```

**Khi nào cần sửa:**
- Thêm global authorization
- Custom error pages
- Logging filter

---

#### 📄 **IdentityConfig.cs** - ASP.NET Identity

**Chức năng:** Cấu hình authentication/authorization (đăng nhập, phân quyền)

**Nội dung:** Cấu hình password policy, email service, user manager...

**Khi nào cần sửa:**
- Đổi yêu cầu password (độ dài, ký tự đặc biệt)
- Cấu hình email confirmation
- Two-factor authentication

---

### 3️⃣ **Areas/** - Admin Panel ⭐ QUAN TRỌNG

**Chức năng:** Tách riêng phần admin ra khỏi phần user (module hóa)

**Cấu trúc:**

```
Areas/
└── admin/                          ← Admin area
    ├── adminAreaRegistration.cs    ← Đăng ký area
    ├── Controllers/                ← Admin controllers
    │   ├── AccountController.cs
    │   ├── AuthController.cs
    │   ├── CategoriesController.cs ← Quản lý danh mục
    │   ├── DeliveriesController.cs ← Quản lý giao hàng
    │   ├── HomeController.cs       ← Admin dashboard
    │   ├── OrdersController.cs     ← Quản lý đơn hàng ⭐
    │   ├── Order_itemsController.cs
    │   ├── PaymentsController.cs   ← Quản lý thanh toán
    │   ├── ProductsController.cs   ← Quản lý sản phẩm ⭐
    │   ├── RolesController.cs      ← Quản lý role
    │   ├── TransportsController.cs
    │   └── UsersController.cs      ← Quản lý user ⭐
    └── Views/                      ← Admin views
        ├── Categories/
        ├── Deliveries/
        ├── Home/
        ├── Orders/
        ├── Products/
        ├── Roles/
        ├── Shared/                 ← Layout admin
        ├── Transports/
        └── Users/
```

**URL cho admin area:**
```
/admin/Products              → Danh sách sản phẩm
/admin/Products/Create       → Thêm sản phẩm mới
/admin/Products/Edit/5       → Sửa sản phẩm ID=5
/admin/Products/Delete/5     → Xóa sản phẩm ID=5
/admin/Orders                → Danh sách đơn hàng
/admin/Users                 → Danh sách user
```

**Khi nào cần sửa:**
- Thêm chức năng quản lý mới (báo cáo, thống kê...)
- Thêm authorization (kiểm tra quyền admin)
- Tùy chỉnh giao diện admin

**Ví dụ tạo area mới:**
```
Areas/
├── admin/    ← Hiện có
└── vendor/   ← Thêm area cho nhà cung cấp
```

---

### 4️⃣ **bin/** - Compiled Files

**Chức năng:** Chứa file `.dll` và `.exe` sau khi compile

**Các file quan trọng:**

```
bin/
├── webBH.dll                            ← Code của bạn (compiled)
├── webBH.pdb                            ← Debug symbols
├── EntityFramework.dll                  ← Entity Framework
├── EntityFramework.SqlServer.dll
├── Microsoft.AspNet.Identity.*.dll      ← Identity system
├── Newtonsoft.Json.dll                  ← JSON parsing
├── System.Web.Mvc.dll                   ← ASP.NET MVC
├── Antlr3.Runtime.dll                   ← CSS/JS minification
├── WebGrease.dll                        ← Optimization
└── (nhiều DLLs khác...)
```

**⚠️ KHÔNG cần sửa thư mục này!**
- Tự động generate khi build project
- Không commit vào Git (đã có trong .gitignore)

**Khi nào cần quan tâm:**
- Deploy lên server → copy toàn bộ thư mục bin
- Debug DLL version conflicts
- Check dependencies

---

### 5️⃣ **Content/** - CSS & Static Assets ⭐ QUAN TRỌNG

**Chức năng:** Chứa CSS, images, fonts

**Các file trong thư mục:**

```
Content/
├── CSS Bootstrap:
│   ├── bootstrap.css               ← Bootstrap 3.4.1 (146KB)
│   ├── bootstrap.min.css           ← Minified (121KB)
│   ├── bootstrap-theme.css
│   └── bootstrap-theme.min.css
│
├── CSS Custom:
│   ├── Site.css                    ← CSS CHUNG toàn website ⭐
│   ├── LoginStyle.css              ← CSS trang Login
│   ├── RegisterStyle.css           ← CSS trang Register
│   ├── CartStyle.css               ← CSS giỏ hàng
│   ├── HomeStyle.css               ← CSS trang chủ
│   ├── DetailStyle.css             ← CSS chi tiết sản phẩm
│   └── PagedList.css               ← CSS phân trang
│
└── Icon Font:
    └── all.min.css                 ← Font Awesome icons
```

**Khi nào cần sửa:**
- Thay đổi màu sắc, font chữ
- Custom giao diện
- Thêm CSS mới

**Load CSS trong View:**
```cshtml
<!-- Load bundle -->
@Styles.Render("~/Content/css")

<!-- Load riêng -->
<link href="~/Content/CartStyle.css" rel="stylesheet" />
```

**Xem thêm:** `HUONG_DAN_SUA_CSS_JQUERY.md`

---

### 6️⃣ **Controllers/** - Xử lý Logic ⭐ CỰC KỲ QUAN TRỌNG

**Chức năng:** Nhận request từ user, xử lý business logic, trả về View

**Mô hình MVC:**
```
User Request → Controller → Model (lấy data) → View (render HTML) → Response
```

**Các controller trong project:**

```
Controllers/
├── HomeController.cs          ← Trang chủ, danh sách sản phẩm ⭐
├── LoginController.cs         ← Đăng nhập ⭐
├── RegisterController.cs      ← Đăng ký ⭐
├── LogoutController.cs        ← Đăng xuất
├── CartController.cs          ← Giỏ hàng (AddToCart, Order) ⭐⭐⭐
└── ProductsController.cs      ← Sản phẩm theo category (Ao, Quan)
```

#### 📄 **HomeController.cs** - Trang chủ

**Các action methods:**

```csharp
public class HomeController : Controller
{
    // GET: /Home/Index
    public ActionResult Index(string SearchString = "")
    {
        // Hiển thị danh sách sản phẩm
        // Có chức năng search
    }

    // GET: /Home/Detail/5
    public ActionResult Detail(int? id)
    {
        // Hiển thị chi tiết sản phẩm
    }

    // GET: /Home/About
    public ActionResult About()
    {
        // Trang giới thiệu
    }

    // GET: /Home/Contact
    public ActionResult Contact()
    {
        // Trang liên hệ
    }
}
```

**URL mapping:**
```
/                    → HomeController.Index()
/Home                → HomeController.Index()
/Home/Index          → HomeController.Index()
/Home/Detail/5       → HomeController.Detail(5)
```

---

#### 📄 **LoginController.cs** - Đăng nhập

**Các action methods:**

```csharp
public class LoginController : Controller
{
    // GET: /Login
    public ActionResult Index()
    {
        // Hiển thị form login
    }

    // POST: /Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(User user)
    {
        // Xử lý đăng nhập
        // Kiểm tra email/password
        // Tạo Session
        // SHA256 hash password
    }
}
```

**Luồng hoạt động:**
1. User truy cập `/Login` → GET Index() → Hiển thị form
2. User nhập email/password → POST Index() → Validate
3. Đúng → Tạo Session → Redirect `/Home`
4. Sai → Show error message

---

#### 📄 **CartController.cs** - Giỏ hàng ⭐⭐⭐

**Các action methods:**

```csharp
public class CartController : Controller
{
    // GET: /Cart
    public ActionResult Index()
    {
        // Hiển thị giỏ hàng
    }

    // GET: /Cart/AddToCart?id_product=5&id_user=3
    public ActionResult AddToCart(int? id_product, int id_user)
    {
        // Thêm sản phẩm vào giỏ
    }

    // POST: /Cart/Update
    public ActionResult Update(int? id, int? quantity, int? id_product)
    {
        // Cập nhật số lượng
    }

    // GET: /Cart/Delete/5
    public ActionResult Delete(int? id)
    {
        // Hiển thị confirm delete
    }

    // POST: /Cart/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        // Xóa sản phẩm khỏi giỏ
    }

    // GET: /Cart/Order
    public ActionResult Order()
    {
        // Đặt hàng
        // Tạo Order
        // Tạo Order_items
        // GỬI EMAIL HÓA ĐƠN ← ĐÂY LÀ CHỖ GỬI EMAIL ⭐⭐⭐
        // Xóa giỏ hàng
    }
}
```

**🔥 Chức năng GỬI EMAIL trong Order() - dòng 169-175:**

```csharp
// Dòng 169-175 trong CartController.cs
MailMessage mail = new MailMessage("1951012033hoang@ou.edu.vn", users.email, "Thông tin đơn hàng", sMsg);
SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
mail.Body = sMsg;
mail.IsBodyHtml = true;
client.EnableSsl = true;
client.Credentials = new NetworkCredential("1951012033hoang@ou.edu.vn", "hh0817765357");
client.Send(mail);
```

**Luồng đặt hàng:**
1. User vào giỏ hàng → Bấm "Thanh toán"
2. Gọi `/Cart/Order`
3. Tạo Order trong database
4. Tạo Order_items từ Cart
5. **GỬI EMAIL** chứa HTML table sản phẩm
6. Xóa giỏ hàng
7. Redirect về `/Cart` với message

---

#### 📄 **RegisterController.cs** - Đăng ký

**Các action methods:**

```csharp
public class RegisterController : Controller
{
    // GET: /Register
    public ActionResult Index()
    {
        // Hiển thị form register
    }

    // POST: /Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(User user)
    {
        // Xử lý đăng ký
        // Hash password với SHA256
        // Set role = User (id_roles = 1)
        // Save vào database
    }
}
```

---

#### 📄 **ProductsController.cs** - Sản phẩm theo category

**Các action methods:**

```csharp
public class ProductsController : Controller
{
    // GET: /Products/Ao
    public ActionResult Ao()
    {
        // Hiển thị danh sách Áo (id_category = 1)
    }

    // GET: /Products/Quan
    public ActionResult Quan()
    {
        // Hiển thị danh sách Quần (id_category = 2)
    }
}
```

---

**Khi nào cần sửa Controllers:**
- Thêm chức năng mới
- Sửa business logic
- Thêm validation
- Tối ưu performance
- Fix bugs

**Xem thêm:** `CODE_REVIEW_REPORT.md` để biết các vấn đề cần sửa

---

### 7️⃣ **fonts/** - Font Files

**Chức năng:** Chứa font files (thường là Glyphicons của Bootstrap)

```
fonts/
├── glyphicons-halflings-regular.eot
├── glyphicons-halflings-regular.svg
├── glyphicons-halflings-regular.ttf
├── glyphicons-halflings-regular.woff
└── glyphicons-halflings-regular.woff2
```

**Sử dụng:**
```html
<span class="glyphicon glyphicon-shopping-cart"></span>
```

**⚠️ Ít khi cần sửa thư mục này**

---

### 8️⃣ **Images/** - Hình ảnh sản phẩm ⭐

**Chức năng:** Chứa hình ảnh sản phẩm, background, logo...

**Các file trong thư mục:**
```
Images/
├── background-login.jpg      ← Background trang login
├── product-1.jpg             ← Ảnh sản phẩm
├── product-2.jpg
├── product-3.jpg
├── ...
└── no-image.png              ← Ảnh mặc định (nếu có)
```

**Sử dụng trong View:**

```cshtml
<!-- Trong cshtml -->
<img src="~/Images/product-1.jpg" alt="Product" />

<!-- Hoặc từ database -->
<img src="@Url.Content("~/" + Model.image)" alt="" />
<!-- Model.image = "Images/product-1.jpg" -->
```

**Sử dụng trong CSS:**

```css
/* Trong LoginStyle.css */
section {
    background: url('/Images/background-login.jpg') no-repeat;
}
```

**Khi nào cần sửa:**
- Upload ảnh sản phẩm mới
- Thay background
- Thêm logo

**⚠️ Lưu ý:**
- Nên tối ưu kích thước ảnh (< 500KB)
- Dùng format WebP cho performance tốt hơn
- Đặt tên file rõ ràng (product-ao-thun-1.jpg)

---

### 9️⃣ **Models/** - Dữ liệu & Entity Framework ⭐⭐⭐

**Chức năng:** Định nghĩa cấu trúc dữ liệu, database entities, business logic

**Các file trong thư mục:**

```
Models/
├── Entity Framework (Database First):
│   ├── BanHangEF.edmx              ← Entity Data Model ⭐
│   ├── BanHangEF.edmx.diagram      ← Visual diagram
│   ├── BanHangEF.Context.cs        ← DbContext
│   ├── BanHangEF.Context.tt        ← T4 Template
│   ├── BanHangEF.Designer.cs
│   ├── BanHangEF.cs
│   └── BanHangEF.tt                ← T4 Template
│
├── Entity Classes (Auto-generated từ database):
│   ├── User.cs                     ← User entity ⭐
│   ├── Product.cs                  ← Product entity ⭐
│   ├── Category.cs                 ← Category entity
│   ├── Order.cs                    ← Order entity ⭐
│   ├── Order_items.cs              ← Order items entity
│   ├── Cart.cs                     ← Cart entity ⭐
│   ├── Payment.cs                  ← Payment entity
│   ├── Delivery.cs                 ← Delivery entity
│   ├── Transport.cs                ← Transport entity
│   ├── Role.cs                     ← Role entity
│   ├── Review.cs                   ← Review entity
│   └── sysdiagram.cs               ← Database diagram
│
├── ViewModels (ASP.NET Identity):
│   ├── AccountViewModels.cs        ← Login/Register VMs
│   ├── ManageViewModels.cs         ← Manage account VMs
│   └── IdentityModels.cs           ← Identity models
│
├── Custom Classes:
│   ├── CustomRoleProvider.cs       ← Role provider ⭐
│   ├── AuthUser.cs                 ← Authentication user
│   ├── GetSessionValue.cs          ← Session helper
│   └── ImageHelper.cs              ← Image utilities
```

---

#### 📊 **Entity Framework Models**

**BanHangEF.edmx** là file chính, chứa mapping giữa database và C# classes.

**Các Entity chính:**

##### **User.cs** - Người dùng

```csharp
public partial class User
{
    public int id { get; set; }
    public string name { get; set; }
    public Nullable<DateTime> birthday { get; set; }
    public Nullable<int> sex { get; set; }
    public string email { get; set; }
    public string password { get; set; }    // SHA256 hashed
    public int id_roles { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; }
    public virtual ICollection<Order> Orders { get; set; }
    public virtual ICollection<Cart> Carts { get; set; }
}
```

---

##### **Product.cs** - Sản phẩm

```csharp
public partial class Product
{
    public int id { get; set; }
    public string name { get; set; }
    public Nullable<int> price { get; set; }
    public string size { get; set; }
    public string color { get; set; }
    public string image { get; set; }       // Path: "Images/product-1.jpg"
    public Nullable<int> id_category { get; set; }

    // Navigation properties
    public virtual Category Category { get; set; }
    public virtual ICollection<Cart> Carts { get; set; }
    public virtual ICollection<Order_items> Order_items { get; set; }
}
```

---

##### **Order.cs** - Đơn hàng

```csharp
public partial class Order
{
    public int id { get; set; }
    public Nullable<DateTime> date { get; set; }
    public Nullable<int> total_money { get; set; }
    public Nullable<int> id_user { get; set; }
    public Nullable<int> id_payment { get; set; }
    public Nullable<int> id_delivery { get; set; }

    // Navigation properties
    public virtual User User { get; set; }
    public virtual Payment Payment { get; set; }
    public virtual Delivery Delivery { get; set; }
    public virtual ICollection<Order_items> Order_items { get; set; }
}
```

---

##### **Cart.cs** - Giỏ hàng

```csharp
public partial class Cart
{
    public int id { get; set; }
    public Nullable<int> id_product { get; set; }
    public Nullable<int> id_user { get; set; }
    public Nullable<int> quantity { get; set; }
    public Nullable<int> total_money { get; set; }  // quantity * price

    // Navigation properties
    public virtual Product Product { get; set; }
    public virtual User User { get; set; }
}
```

---

##### **Category.cs** - Danh mục

```csharp
public partial class Category
{
    public int id { get; set; }
    public string name { get; set; }    // "Áo", "Quần"

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; }
}
```

---

#### 🛠️ **Custom Classes**

##### **CustomRoleProvider.cs** - Phân quyền

**Chức năng:** Implement role-based authorization

```csharp
public class CustomRoleProvider : RoleProvider
{
    public override string[] GetRolesForUser(string username)
    {
        // Lấy roles của user từ database
        // Return: ["Admin"] hoặc ["User"]
    }

    public override bool IsUserInRole(string username, string roleName)
    {
        // Check user có role không
    }
}
```

**Sử dụng:**
```csharp
// Trong Web.config
<roleManager enabled="true" defaultProvider="CustomRoleProvider">
    <providers>
        <add name="CustomRoleProvider" type="webBH.Models.CustomRoleProvider"/>
    </providers>
</roleManager>
```

---

##### **GetSessionValue.cs** - Session helper

**Chức năng:** Helper để lấy giá trị từ Session

```csharp
public static class GetSessionValue
{
    public static string GetSession(string key)
    {
        // Lấy giá trị từ Session
    }
}
```

---

#### 🔄 **Cách hoạt động của Entity Framework**

**Database First approach:**

```
SQL Server Database
        ↓
    (Generate)
        ↓
BanHangEF.edmx (EDMX file)
        ↓
    (T4 Template)
        ↓
C# Entity Classes (User.cs, Product.cs...)
```

**Sử dụng trong Controller:**

```csharp
public class HomeController : Controller
{
    private webBHEntities db = new webBHEntities();  // DbContext

    public ActionResult Index()
    {
        // LINQ query
        var products = db.Products
            .Include(p => p.Category)    // JOIN
            .Where(p => p.price > 100000)  // WHERE
            .OrderBy(p => p.name)        // ORDER BY
            .ToList();

        return View(products);
    }
}
```

---

**Khi nào cần sửa Models:**
- Thêm thuộc tính mới vào entity → Sửa database → Update EDMX
- Thêm validation rules
- Thêm business logic methods
- Tạo ViewModel mới

**⚠️ Lưu ý:**
- KHÔNG sửa trực tiếp các file entity (User.cs, Product.cs...) vì sẽ bị overwrite khi update EDMX
- Dùng **partial class** để extend entity

**Ví dụ extend entity:**

```csharp
// File: Models/User.Validation.cs (tạo mới)
using System.ComponentModel.DataAnnotations;

namespace webBH.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User  // Extend User entity
    {
        // Thêm properties/methods mới ở đây
    }

    public class UserMetadata
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        [MinLength(6)]
        public string password { get; set; }
    }
}
```

---

### 🔟 **obj/** - Object Files (Temporary)

**Chức năng:** Chứa file tạm trong quá trình compile

```
obj/
├── Debug/
│   ├── *.cache
│   ├── *.pdb
│   └── edmxResourcesToEmbed/
└── Release/
```

**⚠️ KHÔNG cần sửa thư mục này!**
- Tự động generate
- Không commit vào Git

---

### 1️⃣1️⃣ **Properties/** - Assembly Info

**Chức năng:** Thông tin về assembly (version, copyright...)

```
Properties/
└── AssemblyInfo.cs
```

**Nội dung:**

```csharp
[assembly: AssemblyTitle("webBH")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("webBH")]
[assembly: AssemblyCopyright("Copyright © 2023")]
[assembly: AssemblyVersion("1.0.0.0")]
```

**Khi nào cần sửa:**
- Cập nhật version number
- Thay đổi copyright
- Thêm description

---

### 1️⃣2️⃣ **Scripts/** - JavaScript/jQuery ⭐

**Chức năng:** Chứa JavaScript libraries

```
Scripts/
├── jQuery:
│   ├── jquery-3.4.1.js              ← jQuery 3.4.1 (280KB)
│   ├── jquery-3.4.1.min.js          ← Minified (88KB)
│   ├── jquery-3.4.1.slim.js         ← Slim (không AJAX)
│   └── jquery-3.4.1.slim.min.js
│
├── Bootstrap:
│   ├── bootstrap.js                 ← Bootstrap JS (75KB)
│   └── bootstrap.min.js             ← Minified (40KB)
│
├── Validation:
│   ├── jquery.validate.js           ← jQuery Validate
│   ├── jquery.validate.min.js
│   ├── jquery.validate.unobtrusive.js
│   └── jquery.validate.unobtrusive.min.js
│
└── Other:
    └── modernizr-2.8.3.js           ← Feature detection
```

**Khi nào cần sửa:**
- Thêm jQuery plugin mới
- Viết custom JavaScript
- Update jQuery version

**Load JavaScript trong View:**

```cshtml
<!-- Trong _Layout.cshtml -->
@Scripts.Render("~/bundles/jquery")
@Scripts.Render("~/bundles/bootstrap")

<!-- Load riêng -->
<script src="~/Scripts/custom.js"></script>
```

**Xem thêm:** `HUONG_DAN_SUA_CSS_JQUERY.md`

---

### 1️⃣3️⃣ **Views/** - Giao diện HTML ⭐⭐⭐

**Chức năng:** Chứa file .cshtml (Razor views) - giao diện hiển thị cho user

**Cấu trúc:**

```
Views/
├── Shared/                       ← Views dùng chung
│   ├── _Layout.cshtml           ← Template chính (header, footer) ⭐
│   ├── LayoutBlank.cshtml       ← Template không header/footer
│   ├── _LoginPartial.cshtml     ← User menu partial
│   ├── Error.cshtml             ← Error page
│   └── Lockout.cshtml
│
├── Home/                         ← Views của HomeController
│   ├── Index.cshtml             ← Trang chủ ⭐
│   ├── Detail.cshtml            ← Chi tiết sản phẩm
│   ├── About.cshtml
│   └── Contact.cshtml
│
├── Login/                        ← Views của LoginController
│   └── Index.cshtml             ← Form login ⭐
│
├── Register/                     ← Views của RegisterController
│   └── Index.cshtml             ← Form register ⭐
│
├── Cart/                         ← Views của CartController
│   ├── Index.cshtml             ← Giỏ hàng ⭐
│   └── Delete.cshtml            ← Confirm delete
│
├── Products/                     ← Views của ProductsController
│   ├── Ao.cshtml                ← Danh sách áo
│   └── Quan.cshtml              ← Danh sách quần
│
├── Account/                      ← ASP.NET Identity views
│   ├── Login.cshtml
│   ├── Register.cshtml
│   ├── ForgotPassword.cshtml
│   └── ...
│
├── Manage/                       ← Account management
│
├── _ViewStart.cshtml             ← Set default layout ⭐
└── Web.config                    ← View config (chặn truy cập trực tiếp)
```

---

#### 📄 **_ViewStart.cshtml** - Default Layout

**Chức năng:** Set layout mặc định cho tất cả views

```cshtml
@{
    Layout = "~/Views/Shared/_Layout.cshtml";
}
```

Mọi view đều tự động dùng `_Layout.cshtml` trừ khi override.

---

#### 📄 **Shared/_Layout.cshtml** - Template chính ⭐

**Chức năng:** Template chung (header, navbar, footer) cho toàn website

**Cấu trúc:**

```cshtml
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>@ViewBag.Title - My ASP.NET Application</title>
    @Styles.Render("~/Content/css")
</head>
<body>
    <!-- Navbar -->
    <div class="navbar navbar-inverse navbar-fixed-top">
        <div class="container">
            @Html.ActionLink("OU SHOP", "Index", "Home")

            <ul class="nav navbar-nav">
                <li>@Html.ActionLink("Trang chủ", "Index", "Home")</li>
                <li>@Html.ActionLink("Áo", "Ao", "Products")</li>
                <li>@Html.ActionLink("Quần", "Quan", "Products")</li>
            </ul>

            @Html.Partial("_LoginPartial")  <!-- User menu -->
        </div>
    </div>

    <!-- Body content -->
    <div class="container body-content">
        @RenderBody()  ← View con được insert vào đây ⭐

        <footer>
            <p>&copy; @DateTime.Now.Year - Group 17</p>
        </footer>
    </div>

    @Scripts.Render("~/bundles/jquery")
    @Scripts.Render("~/bundles/bootstrap")
    @RenderSection("scripts", required: false)  ← Scripts từ view con
</body>
</html>
```

**@RenderBody()** là nơi view con được render vào.

**Ví dụ:**
- User truy cập `/Home/Index`
- Render `Views/Home/Index.cshtml`
- Insert vào `@RenderBody()` của `_Layout.cshtml`

---

#### 📄 **Home/Index.cshtml** - Trang chủ

**Chức năng:** Hiển thị danh sách sản phẩm

```cshtml
@model IEnumerable<webBH.Models.Product>

@{
    ViewBag.Title = "Trang chủ";
}

@Styles.Render("~/Content/HomeStyle.css")

<div class="products">
    @foreach (var item in Model)
    {
        <div class="product">
            <img src="@Url.Content("~/" + item.image)" alt="" />
            <h3>@item.name</h3>
            <span class="price">@item.price đ</span>

            <div class="footer">
                @Html.ActionLink("Xem chi tiết", "Detail", new { id = item.id })

                @if (Session["UserId"] != null)
                {
                    @Html.ActionLink("Thêm vào giỏ", "AddToCart", "Cart",
                        new { id_product = item.id, id_user = Session["UserId"] }, null)
                }
                else
                {
                    @Html.ActionLink("Thêm vào giỏ", "Index", "Login")
                }
            </div>
        </div>
    }
</div>
```

---

#### 📄 **Cart/Index.cshtml** - Giỏ hàng

**Chức năng:** Hiển thị giỏ hàng, cho phép update/delete

```cshtml
@model List<webBH.Models.Cart>

@Styles.Render("~/Content/CartStyle.css")

<div class="shopping-cart">
    @foreach (var item in Model)
    {
        <div class="product">
            <img src="@Url.Content("~/" + item.Product.image)" />
            <div>@item.Product.name</div>
            <div>@item.Product.price đ</div>

            <form action="@Url.Action("Update", "Cart")" method="post">
                <input type="number" name="quantity" value="@item.quantity" />
                <input type="submit" value="Cập nhật" />
            </form>

            @Html.ActionLink("Xóa", "Delete", new { id = item.id })
        </div>
    }

    <div class="totals">
        <div>Tổng tiền: @ViewBag.total đ</div>
    </div>

    @Html.ActionLink("Thanh toán", "Order", null, new { @class = "checkout" })
</div>

<script>
    var message = '@TempData["Message"]';
    if (message) {
        alert(message);
    }
</script>
```

---

**Khi nào cần sửa Views:**
- Thay đổi giao diện
- Thêm HTML/CSS mới
- Sửa layout
- Thêm JavaScript

**Xem thêm:** `HUONG_DAN_SUA_CSHTML.md`

---

## 📄 CÁC FILE QUAN TRỌNG Ở ROOT

### **Global.asax** - Application Startup

**Chức năng:** Entry point của ứng dụng, chạy khi app khởi động

```csharp
// File: Global.asax.cs

public class MvcApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        AreaRegistration.RegisterAllAreas();        // Đăng ký Areas
        FilterConfig.RegisterGlobalFilters(...);    // Đăng ký Filters
        RouteConfig.RegisterRoutes(...);            // Đăng ký Routes
        BundleConfig.RegisterBundles(...);          // Đăng ký Bundles
    }

    protected void Application_Error()
    {
        // Global error handling
    }

    protected void Session_Start()
    {
        // Chạy khi session mới được tạo
    }
}
```

**Khi nào cần sửa:**
- Thêm global error handling
- Initialize services
- Set default culture/language

---

### **Web.config** - Cấu hình chính ⭐⭐⭐

**Chức năng:** File cấu hình chính của ứng dụng

**Các phần quan trọng:**

#### 1. Connection Strings

```xml
<connectionStrings>
    <add name="webBHEntities"
         connectionString="data source=RUOIGIAODIEN\HOANG;
                          initial catalog=qlbanhang_12032023;
                          integrated security=True;..."
         providerName="System.Data.EntityClient" />
</connectionStrings>
```

**Khi nào cần sửa:** Đổi database server, database name

---

#### 2. App Settings

```xml
<appSettings>
    <add key="webpages:Version" value="3.0.0.0" />
    <add key="ClientValidationEnabled" value="true" />
    <add key="UnobtrusiveJavaScriptEnabled" value="true" />

    <!-- THÊM EMAIL SETTINGS ĐỂ FIX LỖI HARDCODED PASSWORD -->
    <add key="EmailFrom" value="your-email@gmail.com" />
    <add key="EmailAppPassword" value="your-app-password" />
</appSettings>
```

---

#### 3. Authentication & Authorization

```xml
<system.web>
    <!-- Role Provider -->
    <roleManager enabled="true" defaultProvider="CustomRoleProvider">
        <providers>
            <clear/>
            <add name="CustomRoleProvider" type="webBH.Models.CustomRoleProvider"/>
        </providers>
    </roleManager>

    <!-- Forms Authentication -->
    <authentication mode="Forms">
        <forms loginUrl="~/Login" timeout="2880" />
    </authentication>
</system.web>
```

---

#### 4. Compilation & HTTP Runtime

```xml
<system.web>
    <compilation debug="true" targetFramework="4.7.2" />
    <httpRuntime targetFramework="4.7.2" />
</system.web>
```

**⚠️ Lưu ý:**
- `debug="true"` → Development (chậm hơn)
- `debug="false"` → Production (nhanh hơn)

---

### **packages.config** - NuGet Packages

**Chức năng:** Liệt kê các NuGet packages được cài đặt

```xml
<packages>
    <package id="EntityFramework" version="6.2.0" targetFramework="net472" />
    <package id="jQuery" version="3.4.1" targetFramework="net472" />
    <package id="bootstrap" version="3.4.1" targetFramework="net472" />
    <package id="Microsoft.AspNet.Mvc" version="5.2.7" targetFramework="net472" />
    <!-- ... nhiều packages khác -->
</packages>
```

**Khi nào cần sửa:**
- Install/Uninstall package qua NuGet Package Manager
- Update package version

---

### **webBH.csproj** - Project File

**Chức năng:** Định nghĩa cấu trúc project, references, build settings

**⚠️ Ít khi cần sửa trực tiếp**
- Visual Studio tự động quản lý
- Chỉ sửa khi có conflict hoặc cần custom build

---

## 🔄 LUỒNG HOẠT ĐỘNG CỦA PROJECT

### Ví dụ: User đặt hàng

```
1. User click "Thanh toán" trong giỏ hàng
        ↓
2. Browser gửi request: GET /Cart/Order
        ↓
3. RouteConfig map URL → CartController.Order()
        ↓
4. CartController.Order():
   - Lấy giỏ hàng từ database (Models/Cart.cs)
   - Tạo Order mới
   - Tạo Order_items
   - GỬI EMAIL (System.Net.Mail)
   - Xóa giỏ hàng
   - Set TempData["Message"]
        ↓
5. Redirect về /Cart/Index
        ↓
6. CartController.Index():
   - Load giỏ hàng (trống)
   - Return View(cart)
        ↓
7. Render Views/Cart/Index.cshtml
   - Insert vào _Layout.cshtml
   - Show alert với TempData["Message"]
        ↓
8. Response HTML về browser
```

---

## 📚 TÀI LIỆU THAM KHẢO THEO THƯ MỤC

| Thư mục | Xem thêm |
|---------|----------|
| **Content/** (CSS) | `HUONG_DAN_SUA_CSS_JQUERY.md` |
| **Scripts/** (jQuery) | `HUONG_DAN_SUA_CSS_JQUERY.md` |
| **Views/** (.cshtml) | `HUONG_DAN_SUA_CSHTML.md` |
| **Controllers/**, **Models/** | `CODE_REVIEW_REPORT.md` |

---

## 🎯 CHECKLIST: THÊM CHỨC NĂNG MỚI

Ví dụ: Thêm chức năng "Wishlist" (danh sách yêu thích)

### Bước 1: Database
- [ ] Tạo table `Wishlist` trong SQL Server
- [ ] Columns: `id`, `id_user`, `id_product`, `date_added`

### Bước 2: Models
- [ ] Update EDMX: Right click → Update Model from Database
- [ ] File `Wishlist.cs` tự động generate

### Bước 3: Controller
- [ ] Tạo `Controllers/WishlistController.cs`
- [ ] Actions: `Index()`, `Add()`, `Remove()`

### Bước 4: Views
- [ ] Tạo folder `Views/Wishlist/`
- [ ] Tạo `Index.cshtml`

### Bước 5: Navigation
- [ ] Thêm link vào `_Layout.cshtml`:
```cshtml
<li>@Html.ActionLink("Yêu thích", "Index", "Wishlist")</li>
```

### Bước 6: CSS/JS (Optional)
- [ ] Tạo `Content/WishlistStyle.css`
- [ ] Load trong view

### Bước 7: Test
- [ ] Test thêm/xóa
- [ ] Test với nhiều users
- [ ] Test responsive

---

## ❓ FAQ

### Q: Tôi nên sửa file nào khi muốn đổi giao diện?
**A:** Sửa file `.cshtml` trong thư mục `Views/` và CSS trong `Content/`

### Q: Tôi nên sửa file nào khi muốn thêm chức năng mới?
**A:**
1. Tạo Controller trong `Controllers/`
2. Tạo Model (hoặc update EDMX)
3. Tạo View trong `Views/`

### Q: File nào chứa logic gửi email?
**A:** `Controllers/CartController.cs`, method `Order()`, dòng 169-175

### Q: Làm sao update database schema?
**A:**
1. Sửa database trong SQL Server
2. Mở `BanHangEF.edmx`
3. Right click → Update Model from Database

### Q: Tôi có thể xóa thư mục `bin/` và `obj/` không?
**A:** Có, chúng sẽ được tạo lại khi build. Nhưng không cần thiết.

### Q: File config nào quan trọng nhất?
**A:** `Web.config` (connection string, app settings, authentication)

### Q: Làm sao thêm jQuery plugin mới?
**A:**
1. Copy file vào `Scripts/`
2. Thêm vào `BundleConfig.cs` (optional)
3. Load trong View

---

## 🎓 BÀI TẬP THỰC HÀNH

### Bài 1: Tìm file
- [ ] File chứa logic đăng nhập?
- [ ] File hiển thị trang chủ?
- [ ] File CSS của giỏ hàng?
- [ ] File gửi email hóa đơn?

### Bài 2: Trace luồng code
- [ ] User click "Thêm vào giỏ" → đi qua những file nào?
- [ ] User đăng nhập → password được hash ở đâu?

### Bài 3: Thêm chức năng mới
- [ ] Thêm button "Mua ngay" vào trang chi tiết sản phẩm
- [ ] Thêm trang "Đơn hàng của tôi"
- [ ] Thêm chức năng "Đánh giá sản phẩm"

---

**Tóm lại:** Project này tuân theo kiến trúc MVC chuẩn của ASP.NET. Hiểu rõ từng thư mục sẽ giúp bạn dễ dàng maintain và mở rộng chức năng!

Có thắc mắc gì cứ hỏi tôi nhé! 😊
