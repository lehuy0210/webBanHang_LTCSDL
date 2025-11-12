# 🎓 LỘ TRÌNH CẢI THIỆN PROJECT DỰA TRÊN ĐỀ CƯƠNG MÔN HỌC

## 📋 PHÂN TÍCH PROJECT HIỆN TẠI

### Công nghệ đang dùng:
- ✅ **ASP.NET MVC 5** (Framework cũ, ra mắt 2013)
- ✅ **Entity Framework 6** - Database First Approach
- ✅ **SQL Server** - CSDL quan hệ
- ✅ **Bootstrap 3.4.1** (phiên bản cũ, hiện tại là v5.3)
- ✅ **jQuery 3.4.1** (cũ, hiện tại là v3.7+)
- ✅ **C# 7.2** (.NET Framework 4.7.2)

### Điểm mạnh:
- ✅ Cấu trúc MVC rõ ràng
- ✅ Dùng Entity Framework (ORM)
- ✅ Có phân quyền Admin/User
- ✅ Có chức năng gửi email
- ✅ Có CRUD đầy đủ

### Điểm yếu (từ Code Review):
- ❌ Hardcoded password trong code
- ❌ Thiếu authorization cho admin area
- ❌ Code duplication (không DRY)
- ❌ Magic numbers
- ❌ Không có kiến trúc phân lớp rõ ràng (DTO, DAL, BLL)
- ❌ Không có Repository Pattern
- ❌ Không có Dependency Injection
- ❌ Thiếu Stored Procedures, Triggers
- ❌ Không có Web API
- ❌ Frontend cũ (Bootstrap 3, jQuery)

---

## 🎯 ÁNH XẠ VỚI ĐỀ CƯƠNG MÔN HỌC

### 📚 **MÔN 1: LẬP TRÌNH CƠ SỞ DỮ LIỆU (ITEC3406)**

#### **Chương 1: T-SQL** → ❌ Project THIẾU

| Nội dung đề cương | Trạng thái trong project | Độ ưu tiên |
|-------------------|--------------------------|------------|
| Stored Procedures | ❌ Không có | 🔴 CAO |
| User-Defined Functions | ❌ Không có | 🟡 TRUNG BÌNH |
| Triggers | ❌ Không có | 🟡 TRUNG BÌNH |
| Transactions | ❌ Không có (dùng EF SaveChanges) | 🟢 THẤP |
| Views | ❌ Không có | 🟢 THẤP |

**💡 Đề xuất cải thiện:**

**1. Thêm Stored Procedures**
```sql
-- File: qlbanhang.sql (thêm vào)

-- SP1: Lấy danh sách sản phẩm theo category
CREATE PROCEDURE sp_GetProductsByCategory
    @id_category INT
AS
BEGIN
    SELECT p.*, c.name AS category_name
    FROM Products p
    INNER JOIN Categories c ON p.id_category = c.id
    WHERE p.id_category = @id_category
END
GO

-- SP2: Đặt hàng (thay thế logic trong CartController)
CREATE PROCEDURE sp_CreateOrder
    @id_user INT,
    @id_payment INT,
    @id_delivery INT,
    @order_id INT OUTPUT
AS
BEGIN
    BEGIN TRANSACTION
    BEGIN TRY
        -- Tạo order
        INSERT INTO Orders (date, total_money, id_user, id_payment, id_delivery)
        VALUES (GETDATE(),
                (SELECT SUM(total_money) FROM Cart WHERE id_user = @id_user),
                @id_user, @id_payment, @id_delivery)

        SET @order_id = SCOPE_IDENTITY()

        -- Tạo order_items từ cart
        INSERT INTO Order_items (quanlity, id_order, id_product, total_money)
        SELECT quantity, @order_id, id_product, total_money
        FROM Cart
        WHERE id_user = @id_user

        -- Xóa cart
        DELETE FROM Cart WHERE id_user = @id_user

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO

-- SP3: Tìm kiếm sản phẩm
CREATE PROCEDURE sp_SearchProducts
    @keyword NVARCHAR(100)
AS
BEGIN
    SELECT * FROM Products
    WHERE name LIKE '%' + @keyword + '%'
    ORDER BY name
END
GO

-- SP4: Thống kê doanh thu theo tháng
CREATE PROCEDURE sp_RevenueByMonth
    @year INT,
    @month INT
AS
BEGIN
    SELECT
        DAY(date) AS day,
        COUNT(*) AS total_orders,
        SUM(total_money) AS revenue
    FROM Orders
    WHERE YEAR(date) = @year AND MONTH(date) = @month
    GROUP BY DAY(date)
    ORDER BY day
END
GO
```

**Sử dụng SP trong C#:**
```csharp
// File: Models/OrderRepository.cs (tạo mới)

public class OrderRepository
{
    private webBHEntities db = new webBHEntities();

    public int CreateOrder(int userId, int paymentId, int deliveryId)
    {
        var orderIdParam = new SqlParameter
        {
            ParameterName = "@order_id",
            SqlDbType = SqlDbType.Int,
            Direction = ParameterDirection.Output
        };

        db.Database.ExecuteSqlCommand(
            "EXEC sp_CreateOrder @id_user, @id_payment, @id_delivery, @order_id OUTPUT",
            new SqlParameter("@id_user", userId),
            new SqlParameter("@id_payment", paymentId),
            new SqlParameter("@id_delivery", deliveryId),
            orderIdParam
        );

        return (int)orderIdParam.Value;
    }
}
```

---

**2. Thêm Triggers** (tự động cập nhật inventory, log changes)

```sql
-- Trigger: Cập nhật tổng tiền khi thêm order_items
CREATE TRIGGER trg_UpdateOrderTotal
ON Order_items
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    UPDATE Orders
    SET total_money = (
        SELECT SUM(total_money)
        FROM Order_items
        WHERE id_order = Orders.id
    )
    WHERE id IN (SELECT DISTINCT id_order FROM inserted)
       OR id IN (SELECT DISTINCT id_order FROM deleted)
END
GO

-- Trigger: Log thay đổi giá sản phẩm
CREATE TABLE ProductPriceHistory (
    id INT PRIMARY KEY IDENTITY,
    id_product INT,
    old_price INT,
    new_price INT,
    changed_date DATETIME DEFAULT GETDATE()
)
GO

CREATE TRIGGER trg_LogPriceChange
ON Products
AFTER UPDATE
AS
BEGIN
    IF UPDATE(price)
    BEGIN
        INSERT INTO ProductPriceHistory (id_product, old_price, new_price)
        SELECT d.id, d.price, i.price
        FROM deleted d
        INNER JOIN inserted i ON d.id = i.id
        WHERE d.price <> i.price
    END
END
GO
```

---

**3. Thêm User-Defined Functions**

```sql
-- Function: Tính tổng tiền đơn hàng của user
CREATE FUNCTION fn_GetUserTotalSpent(@id_user INT)
RETURNS INT
AS
BEGIN
    DECLARE @total INT
    SELECT @total = ISNULL(SUM(total_money), 0)
    FROM Orders
    WHERE id_user = @id_user
    RETURN @total
END
GO

-- Sử dụng:
SELECT dbo.fn_GetUserTotalSpent(3) AS total_spent

-- Function: Kiểm tra user có phải VIP không (chi > 10 triệu)
CREATE FUNCTION fn_IsVIPUser(@id_user INT)
RETURNS BIT
AS
BEGIN
    DECLARE @total INT
    SET @total = dbo.fn_GetUserTotalSpent(@id_user)
    RETURN CASE WHEN @total > 10000000 THEN 1 ELSE 0 END
END
GO
```

**Sử dụng trong LINQ:**
```csharp
// Thêm vào BanHangEF.edmx (Function Import)
var totalSpent = db.fn_GetUserTotalSpent(userId).FirstOrDefault();
```

---

#### **Chương 2: ADO.NET** → ⚠️ Project dùng EF (tốt hơn)

**Trạng thái:** Project dùng Entity Framework nên KHÔNG CẦN ADO.NET thuần.

**💡 Lưu ý:** EF đã abstract ADO.NET, nhưng nếu cần performance cao cho queries phức tạp, có thể dùng `SqlCommand` trực tiếp.

---

#### **Chương 3: Kiến trúc đa lớp** → ❌ Project THIẾU rõ ràng

**Trạng thái hiện tại:**
- Controllers: Vừa xử lý logic, vừa truy cập DB trực tiếp
- Models: Chỉ là Entity classes
- Không có lớp DTO, DAL, BLL riêng biệt

**💡 Đề xuất: Refactor sang kiến trúc 3 lớp**

**Kiến trúc mới:**
```
webBH/
├── Models/
│   ├── Entities/          ← Entity Framework classes
│   │   ├── User.cs
│   │   ├── Product.cs
│   │   └── ...
│   │
│   ├── DTOs/              ← Data Transfer Objects (TẠO MỚI)
│   │   ├── UserDTO.cs
│   │   ├── ProductDTO.cs
│   │   ├── OrderDTO.cs
│   │   └── CartItemDTO.cs
│   │
│   └── ViewModels/        ← ViewModels cho Views
│       ├── LoginViewModel.cs
│       ├── RegisterViewModel.cs
│       └── CheckoutViewModel.cs
│
├── DAL/                   ← Data Access Layer (TẠO MỚI)
│   ├── Interfaces/
│   │   ├── IUserRepository.cs
│   │   ├── IProductRepository.cs
│   │   └── IOrderRepository.cs
│   │
│   └── Repositories/
│       ├── UserRepository.cs
│       ├── ProductRepository.cs
│       └── OrderRepository.cs
│
├── BLL/                   ← Business Logic Layer (TẠO MỚI)
│   ├── Services/
│   │   ├── UserService.cs
│   │   ├── ProductService.cs
│   │   ├── OrderService.cs
│   │   └── EmailService.cs
│   │
│   └── Validators/
│       ├── UserValidator.cs
│       └── ProductValidator.cs
│
└── Controllers/           ← Presentation Layer (SỬA LẠI)
    ├── HomeController.cs  ← Chỉ gọi Services, không truy cập DB
    ├── CartController.cs
    └── ...
```

---

**Ví dụ cụ thể: Refactor CartController**

**1. Tạo DTO:**
```csharp
// File: Models/DTOs/OrderDTO.cs (TẠO MỚI)

public class OrderDTO
{
    public int UserId { get; set; }
    public int PaymentId { get; set; }
    public int DeliveryId { get; set; }
    public List<CartItemDTO> Items { get; set; }
    public int TotalMoney { get; set; }
}

public class CartItemDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public int TotalMoney { get; set; }
}
```

---

**2. Tạo Repository (DAL):**
```csharp
// File: DAL/Interfaces/IOrderRepository.cs (TẠO MỚI)

public interface IOrderRepository
{
    Order CreateOrder(OrderDTO orderDto);
    Order GetOrderById(int id);
    List<Order> GetOrdersByUserId(int userId);
    void UpdateOrder(Order order);
    void DeleteOrder(int id);
}

// File: DAL/Repositories/OrderRepository.cs (TẠO MỚI)

public class OrderRepository : IOrderRepository
{
    private webBHEntities db;

    public OrderRepository(webBHEntities context)
    {
        db = context;
    }

    public Order CreateOrder(OrderDTO orderDto)
    {
        using (var transaction = db.Database.BeginTransaction())
        {
            try
            {
                // Tạo Order
                var order = new Order
                {
                    date = DateTime.Now,
                    total_money = orderDto.TotalMoney,
                    id_user = orderDto.UserId,
                    id_payment = orderDto.PaymentId,
                    id_delivery = orderDto.DeliveryId
                };
                db.Orders.Add(order);
                db.SaveChanges();

                // Tạo Order_items
                foreach (var item in orderDto.Items)
                {
                    db.Order_items.Add(new Order_items
                    {
                        quanlity = item.Quantity,
                        id_order = order.id,
                        id_product = item.ProductId,
                        total_money = item.TotalMoney
                    });
                }
                db.SaveChanges();

                // Xóa Cart
                var cartItems = db.Carts.Where(c => c.id_user == orderDto.UserId);
                db.Carts.RemoveRange(cartItems);
                db.SaveChanges();

                transaction.Commit();
                return order;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    public List<Order> GetOrdersByUserId(int userId)
    {
        return db.Orders
            .Include(o => o.Order_items)
            .Where(o => o.id_user == userId)
            .OrderByDescending(o => o.date)
            .ToList();
    }

    // ... các methods khác
}
```

---

**3. Tạo Service (BLL):**
```csharp
// File: BLL/Services/OrderService.cs (TẠO MỚI)

public class OrderService
{
    private IOrderRepository orderRepository;
    private IEmailService emailService;

    public OrderService(IOrderRepository orderRepo, IEmailService emailSvc)
    {
        orderRepository = orderRepo;
        emailService = emailSvc;
    }

    public Order CreateOrder(int userId)
    {
        // Validate
        var cartItems = GetCartItems(userId);
        if (cartItems.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty");
        }

        // Tính tổng tiền
        int total = cartItems.Sum(c => c.TotalMoney);

        // Tạo DTO
        var orderDto = new OrderDTO
        {
            UserId = userId,
            PaymentId = 1, // Default
            DeliveryId = 1, // Default
            Items = cartItems,
            TotalMoney = total
        };

        // Tạo order qua Repository
        var order = orderRepository.CreateOrder(orderDto);

        // Gửi email
        emailService.SendOrderConfirmation(order);

        return order;
    }

    private List<CartItemDTO> GetCartItems(int userId)
    {
        // Logic lấy cart từ DB
        // ...
    }
}

// File: BLL/Services/EmailService.cs (TẠO MỚI)

public interface IEmailService
{
    void SendOrderConfirmation(Order order);
}

public class EmailService : IEmailService
{
    private string emailFrom;
    private string emailPassword;

    public EmailService()
    {
        // Đọc từ Web.config (đã fix hardcoded password)
        emailFrom = ConfigurationManager.AppSettings["EmailFrom"];
        emailPassword = ConfigurationManager.AppSettings["EmailAppPassword"];
    }

    public void SendOrderConfirmation(Order order)
    {
        try
        {
            // Build HTML
            string htmlBody = BuildOrderEmailHtml(order);

            // Gửi email
            MailMessage mail = new MailMessage(emailFrom, order.User.email,
                "Xác nhận đơn hàng #" + order.id, htmlBody);

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            mail.IsBodyHtml = true;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(emailFrom, emailPassword);
            client.Send(mail);
        }
        catch (Exception ex)
        {
            // Log error
            ErrorLogger.Log(ex, "Failed to send order confirmation email");
        }
    }

    private string BuildOrderEmailHtml(Order order)
    {
        // Logic build HTML table
        // ...
    }
}
```

---

**4. Refactor Controller:**
```csharp
// File: Controllers/CartController.cs (SỬA LẠI)

public class CartController : Controller
{
    private OrderService orderService;

    public CartController()
    {
        // TODO: Dùng Dependency Injection thay vì new
        var db = new webBHEntities();
        var orderRepo = new OrderRepository(db);
        var emailService = new EmailService();
        orderService = new OrderService(orderRepo, emailService);
    }

    // GET: /Cart/Order
    public ActionResult Order()
    {
        try
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            // Gọi Service - business logic đã được tách riêng
            var order = orderService.CreateOrder(userId);

            TempData["Message"] = "Đặt hàng thành công! Vui lòng kiểm tra email.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Message"] = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex);
            TempData["Message"] = "Có lỗi xảy ra. Vui lòng thử lại.";
        }

        return RedirectToAction("Index");
    }

    // ... các methods khác
}
```

**Lợi ích:**
- ✅ Tách biệt concerns (Controller không biết gì về DB)
- ✅ Dễ test (mock Repository/Service)
- ✅ Dễ maintain và mở rộng
- ✅ Có thể reuse logic (OrderService dùng cho Web + API)

---

#### **Chương 4: LINQ** → ⚠️ Project có dùng nhưng chưa tối ưu

**Trạng thái:** Project có dùng LINQ nhưng còn đơn giản.

**💡 Đề xuất cải thiện:**

**1. Dùng LINQ phức tạp hơn (GroupBy, Join):**
```csharp
// Thống kê sản phẩm bán chạy
var topProducts = db.Order_items
    .GroupBy(oi => oi.id_product)
    .Select(g => new {
        ProductId = g.Key,
        ProductName = g.FirstOrDefault().Product.name,
        TotalQuantity = g.Sum(oi => oi.quanlity),
        TotalRevenue = g.Sum(oi => oi.total_money)
    })
    .OrderByDescending(x => x.TotalQuantity)
    .Take(10)
    .ToList();

// Tìm users chưa mua hàng
var usersWithoutOrders = db.Users
    .Where(u => !u.Orders.Any())
    .Select(u => new { u.id, u.name, u.email })
    .ToList();

// Doanh thu theo tháng
var revenueByMonth = db.Orders
    .Where(o => o.date.Value.Year == 2023)
    .GroupBy(o => o.date.Value.Month)
    .Select(g => new {
        Month = g.Key,
        TotalOrders = g.Count(),
        TotalRevenue = g.Sum(o => o.total_money)
    })
    .OrderBy(x => x.Month)
    .ToList();
```

---

**2. Dùng LINQ Method Syntax thay Query Syntax:**
```csharp
// Query Syntax (ít dùng)
var products = from p in db.Products
               where p.price > 100000
               orderby p.name
               select p;

// Method Syntax (khuyến nghị)
var products = db.Products
    .Where(p => p.price > 100000)
    .OrderBy(p => p.name)
    .ToList();
```

---

**3. Eager Loading để tránh N+1 query:**
```csharp
// BAD - N+1 queries
var orders = db.Orders.ToList();
foreach (var order in orders)
{
    var user = order.User; // Mỗi lần loop lại query DB
}

// GOOD - 1 query với JOIN
var orders = db.Orders
    .Include(o => o.User)
    .Include(o => o.Order_items.Select(oi => oi.Product))
    .ToList();
```

---

#### **Chương 5: Entity Framework** → ✅ Project đã dùng (Database First)

**Trạng thái:** Project dùng EF6 với Database First approach - phù hợp với đồ án học.

**💡 Đề xuất nâng cao:**

**1. Thêm Repository Pattern:**
(Đã nêu ở Chương 3)

---

**2. Thêm Unit of Work Pattern:**
```csharp
// File: DAL/UnitOfWork.cs (TẠO MỚI)

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    void Save();
}

public class UnitOfWork : IUnitOfWork
{
    private webBHEntities context;
    private IUserRepository userRepository;
    private IProductRepository productRepository;
    private IOrderRepository orderRepository;

    public UnitOfWork(webBHEntities context)
    {
        this.context = context;
    }

    public IUserRepository Users
    {
        get
        {
            if (userRepository == null)
                userRepository = new UserRepository(context);
            return userRepository;
        }
    }

    public IProductRepository Products
    {
        get
        {
            if (productRepository == null)
                productRepository = new ProductRepository(context);
            return productRepository;
        }
    }

    public IOrderRepository Orders
    {
        get
        {
            if (orderRepository == null)
                orderRepository = new OrderRepository(context);
            return orderRepository;
        }
    }

    public void Save()
    {
        context.SaveChanges();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}

// Sử dụng:
using (var uow = new UnitOfWork(new webBHEntities()))
{
    var user = uow.Users.GetById(1);
    var orders = uow.Orders.GetOrdersByUserId(1);

    // Tất cả thay đổi được commit cùng lúc
    uow.Save();
}
```

---

**3. Migration (nếu chuyển sang Code First):**
```bash
# Enable Migrations
Enable-Migrations

# Add Migration
Add-Migration InitialCreate

# Update Database
Update-Database
```

---

#### **Chương 6: ASP.NET Core Web API** → ❌ Project KHÔNG có

**Trạng thái:** Project chỉ có MVC, KHÔNG có API.

**💡 Đề xuất: Thêm Web API để mở rộng**

**Lợi ích khi có API:**
- ✅ Mobile app có thể dùng chung backend
- ✅ Frontend SPA (React/Vue) có thể kết nối
- ✅ Tích hợp với hệ thống khác
- ✅ Học được RESTful API design

**Cách thêm API vào project:**

**Option 1: Thêm Web API Controllers trong project hiện tại**

```csharp
// File: Controllers/API/ProductsApiController.cs (TẠO MỚI)

using System.Web.Http;

namespace webBH.Controllers.API
{
    [RoutePrefix("api/products")]
    public class ProductsApiController : ApiController
    {
        private webBHEntities db = new webBHEntities();

        // GET: api/products
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetProducts()
        {
            var products = db.Products
                .Select(p => new {
                    p.id,
                    p.name,
                    p.price,
                    p.image,
                    category = p.Category.name
                })
                .ToList();

            return Ok(products);
        }

        // GET: api/products/5
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetProduct(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.Products.Add(product);
            db.SaveChanges();

            return Created($"api/products/{product.id}", product);
        }

        // PUT: api/products/5
        [HttpPut]
        [Route("{id}")]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult UpdateProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = db.Products.Find(id);
            if (existing == null)
                return NotFound();

            existing.name = product.name;
            existing.price = product.price;
            existing.size = product.size;
            existing.color = product.color;
            existing.id_category = product.id_category;

            db.SaveChanges();
            return Ok(existing);
        }

        // DELETE: api/products/5
        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult DeleteProduct(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
                return NotFound();

            db.Products.Remove(product);
            db.SaveChanges();

            return Ok();
        }
    }
}
```

**Enable Web API trong project:**

```csharp
// File: App_Start/WebApiConfig.cs (TẠO MỚI)

public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        // Enable attribute routing
        config.MapHttpAttributeRoutes();

        // Default route
        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
        );

        // JSON formatter
        config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling
            = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    }
}

// File: Global.asax.cs (SỬA)
protected void Application_Start()
{
    GlobalConfiguration.Configure(WebApiConfig.Register); // ← Thêm dòng này
    AreaRegistration.RegisterAllAreas();
    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
    RouteConfig.RegisterRoutes(RouteTable.Routes);
    BundleConfig.RegisterBundles(BundleTable.Bundles);
}
```

**Test API:**
```
GET    http://localhost:port/api/products
GET    http://localhost:port/api/products/5
POST   http://localhost:port/api/products
PUT    http://localhost:port/api/products/5
DELETE http://localhost:port/api/products/5
```

---

**Option 2: Tạo project ASP.NET Core Web API riêng** (Nâng cao)

Tạo solution mới với ASP.NET Core 6/7/8, dùng chung database.

---

### 📚 **MÔN 2: LẬP TRÌNH HƯỚNG ĐỐI TƯỢNG**

#### **Chương 1: Tổng quan OOP** → ⚠️ Project chưa áp dụng tốt

**4 nguyên lý OOP:**

| Nguyên lý | Trạng thái | Cải thiện |
|-----------|------------|-----------|
| **Encapsulation** (Đóng gói) | ⚠️ Yếu | Thêm properties, private fields |
| **Inheritance** (Kế thừa) | ❌ Không có | Tạo BaseController |
| **Polymorphism** (Đa hình) | ❌ Không có | Dùng Interfaces |
| **Abstraction** (Trừu tượng) | ❌ Không có | Tạo abstract classes |

---

**💡 Cải thiện Encapsulation:**

```csharp
// BAD - Hiện tại trong Models
public partial class User
{
    public string password { get; set; }  // Public password!
}

// GOOD - Nên dùng
public partial class User
{
    private string _password;

    public string Password
    {
        get { return _password; }
        set { _password = HashPassword(value); }
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
```

---

#### **Chương 3: Kế thừa** → ❌ Project chưa dùng

**💡 Đề xuất: Tạo BaseController**

```csharp
// File: Controllers/BaseController.cs (TẠO MỚI)

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
            return Session["UserId"] as int?;
        }
    }

    // Check đăng nhập
    protected bool IsAuthenticated
    {
        get
        {
            return Session["UserId"] != null;
        }
    }

    // Check admin
    protected bool IsAdmin
    {
        get
        {
            return Session["UserRole"]?.ToString() == "Admin";
        }
    }

    // Helper: Show error message
    protected void ShowError(string message)
    {
        TempData["ErrorMessage"] = message;
    }

    // Helper: Show success message
    protected void ShowSuccess(string message)
    {
        TempData["SuccessMessage"] = message;
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

// Sử dụng:
public class HomeController : BaseController  // ← Kế thừa
{
    public ActionResult Index()
    {
        if (!IsAuthenticated)  // ← Dùng property từ base
        {
            return RedirectToAction("Index", "Login");
        }

        var products = db.Products.ToList();
        return View(products);
    }
}

public class CartController : BaseController  // ← Kế thừa
{
    public ActionResult Index()
    {
        int userId = CurrentUserId.Value;  // ← Dùng từ base
        var cart = db.Carts.Where(c => c.id_user == userId).ToList();
        return View(cart);
    }
}
```

---

#### **Chương 4: Đa hình & Interfaces** → ❌ Project chưa dùng

**💡 Đề xuất: Dùng Interfaces cho Services**

```csharp
// File: BLL/Interfaces/IEmailService.cs (TẠO MỚI)

public interface IEmailService
{
    void SendEmail(string to, string subject, string body);
    void SendOrderConfirmation(Order order);
    void SendPasswordReset(string email, string resetLink);
}

// File: BLL/Services/GmailService.cs (TẠO MỚI)

public class GmailService : IEmailService
{
    public void SendEmail(string to, string subject, string body)
    {
        // Implementation cho Gmail
    }

    public void SendOrderConfirmation(Order order)
    {
        // Implementation
    }

    public void SendPasswordReset(string email, string resetLink)
    {
        // Implementation
    }
}

// File: BLL/Services/SendGridService.cs (TẠO MỚI)

public class SendGridService : IEmailService
{
    public void SendEmail(string to, string subject, string body)
    {
        // Implementation cho SendGrid (dịch vụ email khác)
    }

    public void SendOrderConfirmation(Order order)
    {
        // Implementation
    }

    public void SendPasswordReset(string email, string resetLink)
    {
        // Implementation
    }
}

// Sử dụng - Dễ dàng thay đổi implementation
public class OrderService
{
    private IEmailService emailService;

    // Constructor Injection
    public OrderService(IEmailService emailSvc)
    {
        emailService = emailSvc;  // Có thể là Gmail hoặc SendGrid
    }

    public void CreateOrder(Order order)
    {
        // ...
        emailService.SendOrderConfirmation(order);  // Gọi interface
    }
}

// Trong Controller
var orderService = new OrderService(new GmailService());  // Dùng Gmail
// Hoặc
var orderService = new OrderService(new SendGridService());  // Dùng SendGrid
```

**Lợi ích:**
- ✅ Dễ test (mock IEmailService)
- ✅ Dễ thay đổi implementation (Gmail → SendGrid)
- ✅ Loose coupling

---

### 🎨 **MÔN 3: HTML5/CSS3/ES6**

**Đề cương chi tiết:**

#### **HTML5 (12 chương):**
1. Giới Thiệu HTML5
2. Cấu Trúc Cơ Bản HTML
3. Thẻ Văn Bản Và Định Dạng
4. Links Và Navigation
5. Hình Ảnh Và Multimedia
6. Tables
7. **Forms Và Input** ⭐ (Cần cho Login/Register/Cart)
8. **HTML5 Semantic Elements** ⭐⭐ (section, article, header, footer, nav)
9. **HTML5 APIs** ⭐ (LocalStorage, Geolocation, Drag & Drop)
10. Canvas Và SVG
11. **Storage Và Offline** ⭐ (LocalStorage cho giỏ hàng offline)
12. **Best Practices Và Optimization** ⭐⭐ (SEO, Accessibility)

#### **CSS3 (12 chương):**
1. Giới thiệu CSS3
2. **Selectors** ⭐ (class, id, attribute, pseudo-class)
3. Colors Backgrounds
4. **Box Model Sizing** ⭐ (margin, padding, border)
5. Typography Fonts
6. **Flexbox** ⭐⭐⭐ (thay thế float)
7. **Grid Layout** ⭐⭐⭐ (bố cục hiện đại)
8. **Transitions** ⭐⭐ (hover effects)
9. **Animations** ⭐⭐ (fade in, slide, etc.)
10. **Transforms** ⭐ (rotate, scale, translate)
11. **Media Queries Responsive** ⭐⭐⭐ (mobile-first)
12. **Advanced Topics** ⭐⭐ (CSS Variables, Custom Properties)

#### **ES6 (15 chương):**
1. Giới thiệu ES6
2. **Let, Const và Block Scope** ⭐⭐ (thay var)
3. **Arrow Functions** ⭐⭐⭐ (=> syntax)
4. **Template Literals** ⭐⭐ (backticks)
5. **Destructuring** ⭐⭐⭐ (object/array destructuring)
6. **Spread và Rest Operators** ⭐⭐ (...syntax)
7. **Classes** ⭐⭐⭐ (OOP trong JS)
8. **Modules** ⭐⭐ (import/export)
9. **Promises** ⭐⭐⭐ (xử lý async)
10. Default Parameters ⭐
11. Enhanced Object Literals ⭐
12. **Async/Await** ⭐⭐⭐ (AJAX calls)
13. Map, Set, WeakMap, WeakSet ⭐
14. Symbols
15. Iterators và Generators ⭐

#### **Trạng thái Frontend hiện tại:**

| Công nghệ | Version hiện tại | Version mới nhất | Độ lạc hậu |
|-----------|------------------|------------------|------------|
| Bootstrap | 3.4.1 (2019) | 5.3.2 (2023) | 4 năm |
| jQuery | 3.4.1 (2019) | 3.7.1 (2023) | 4 năm |
| JavaScript | ES5 | ES2023 | Cũ |

**💡 Đề xuất nâng cấp dựa trên đề cương:**

#### **1. Modernize HTML5** (Áp dụng Chương 8 - HTML5 Semantic Elements)

**Thêm Semantic HTML5:**
```html
<!-- BAD - Hiện tại -->
<div class="products">
    <div class="product">...</div>
</div>

<!-- GOOD - Semantic HTML5 -->
<main>
    <section class="products">
        <article class="product">
            <header>
                <h2>Áo thun nam</h2>
            </header>
            <figure>
                <img src="..." alt="Áo thun nam">
                <figcaption>Áo thun chất liệu cotton</figcaption>
            </figure>
            <footer>
                <button>Thêm vào giỏ</button>
            </footer>
        </article>
    </section>
</main>
```

**Thêm Meta tags cho SEO:** (Chương 12 - Best Practices)
```html
<!-- File: Views/Shared/_Layout.cshtml -->
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <!-- SEO Meta Tags -->
    <meta name="description" content="Shop quần áo thời trang OU Shop - Giá rẻ, chất lượng cao">
    <meta name="keywords" content="áo thun, quần jean, thời trang, OU Shop">
    <meta name="author" content="OU Shop">

    <!-- Open Graph (Facebook) -->
    <meta property="og:title" content="OU Shop - Thời trang cho mọi người">
    <meta property="og:description" content="Shop quần áo giá rẻ">
    <meta property="og:image" content="@Url.Content("~/Images/og-image.jpg")">
    <meta property="og:url" content="@Request.Url">

    <!-- Twitter Card -->
    <meta name="twitter:card" content="summary_large_image">

    <title>@ViewBag.Title - OU Shop</title>
</head>
```

---

#### **2. Upgrade CSS3** (Áp dụng Chương 6, 7, 8, 9, 10, 11, 12)

**Dùng CSS Variables thay magic colors:** (Chương 12 - Advanced Topics)
```css
/* File: Content/Site.css */

:root {
    /* Colors */
    --primary-color: #3498db;
    --secondary-color: #2ecc71;
    --danger-color: #e74c3c;
    --warning-color: #f39c12;
    --dark-color: #2c3e50;
    --light-color: #ecf0f1;

    /* Spacing */
    --spacing-xs: 5px;
    --spacing-sm: 10px;
    --spacing-md: 20px;
    --spacing-lg: 40px;

    /* Border Radius */
    --border-radius-sm: 4px;
    --border-radius-md: 8px;
    --border-radius-lg: 16px;

    /* Shadows */
    --shadow-sm: 0 2px 4px rgba(0,0,0,0.1);
    --shadow-md: 0 4px 8px rgba(0,0,0,0.15);
    --shadow-lg: 0 8px 16px rgba(0,0,0,0.2);
}

/* Sử dụng */
.btn-primary {
    background-color: var(--primary-color);
    border-radius: var(--border-radius-md);
    box-shadow: var(--shadow-sm);
    padding: var(--spacing-sm) var(--spacing-md);
}

.btn-primary:hover {
    background-color: #2980b9;  /* Darken primary */
    box-shadow: var(--shadow-md);
}
```

**Dùng Flexbox/Grid thay float:** (Chương 6 - Flexbox, Chương 7 - Grid Layout)
```css
/* BAD - Hiện tại (float) */
.product-image {
    float: left;
    width: 20%;
}
.product-details {
    float: left;
    width: 37%;
}

/* GOOD - Flexbox */
.cart-item {
    display: flex;
    gap: 20px;
    align-items: center;
}

.product-image {
    flex: 0 0 100px;
}

.product-details {
    flex: 1;
}

/* GOOD - Grid cho product listing */
.products {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 20px;
}
```

**Thêm Animations:** (Chương 8 - Transitions, Chương 9 - Animations, Chương 10 - Transforms)
```css
/* File: Content/Site.css */

/* Fade in animation */
@keyframes fadeIn {
    from { opacity: 0; transform: translateY(20px); }
    to { opacity: 1; transform: translateY(0); }
}

.product {
    animation: fadeIn 0.5s ease-out;
}

/* Hover scale */
.product:hover {
    transform: scale(1.05);
    transition: transform 0.3s ease;
}

/* Button ripple effect */
.btn {
    position: relative;
    overflow: hidden;
}

.btn::after {
    content: '';
    position: absolute;
    top: 50%;
    left: 50%;
    width: 0;
    height: 0;
    border-radius: 50%;
    background: rgba(255,255,255,0.5);
    transform: translate(-50%, -50%);
    transition: width 0.6s, height 0.6s;
}

.btn:active::after {
    width: 200px;
    height: 200px;
}
```

---

#### **3. Upgrade JavaScript ES6+** (Áp dụng 15 chương ES6)

**Thay thế jQuery bằng Vanilla JS:** (Chương 3 - Arrow Functions, Chương 5 - Destructuring)
```javascript
// BAD - jQuery (cũ)
$(document).ready(function() {
    $('.btn-add-cart').on('click', function() {
        var productId = $(this).data('product-id');
        alert('Added product ' + productId);
    });
});

// GOOD - ES6+ (hiện đại)
document.addEventListener('DOMContentLoaded', () => {
    // Arrow function
    const addToCartButtons = document.querySelectorAll('.btn-add-cart');

    addToCartButtons.forEach(btn => {
        btn.addEventListener('click', (e) => {
            const productId = e.target.dataset.productId;  // Template literal
            showNotification(`Đã thêm sản phẩm ${productId} vào giỏ`);
        });
    });
});

// Async/Await cho AJAX (Chương 12 - Async/Await, Chương 9 - Promises)
async function addToCart(productId) {
    try {
        const response = await fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ id_product: productId })
        });

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }

        const data = await response.json();
        showNotification('Thêm vào giỏ hàng thành công!');
    } catch (error) {
        console.error('Error:', error);
        showNotification('Có lỗi xảy ra!', 'error');
    }
}

// Destructuring (Chương 5 - Destructuring)
const { id, name, price } = product;

// Spread operator (Chương 6 - Spread và Rest Operators)
const newCart = [...cart, newItem];

// Classes (OOP trong JS) - (Chương 7 - Classes)
class ShoppingCart {
    constructor() {
        this.items = [];
    }

    addItem(product, quantity = 1) {
        const existingItem = this.items.find(item => item.id === product.id);

        if (existingItem) {
            existingItem.quantity += quantity;
        } else {
            this.items.push({ ...product, quantity });
        }
    }

    getTotal() {
        return this.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    }
}

const cart = new ShoppingCart();
cart.addItem(product);
console.log(cart.getTotal());
```

---

## 🎯 LỘ TRÌNH CẢI THIỆN ƯU TIÊN

### **GIAI ĐOẠN 1: BẢO MẬT & BUG FIXES (1-2 tuần)** 🔴 URGENT

#### Week 1:
- [x] Fix hardcoded password → Web.config
- [x] Thêm `[Authorize]` cho admin area
- [x] Fix file upload vulnerability
- [x] Add try-catch cho email sending
- [x] Thêm input validation

**File cần sửa:**
- `CartController.cs`
- `Areas/admin/Controllers/*.cs`
- `Web.config`

**Áp dụng kiến thức:** Chương 3 - LTCSDL (Security best practices)

---

### **GIAI ĐOẠN 2: KIẾN TRÚC PHÂN LỚP (2-3 tuần)** 🟠 HIGH

#### Week 2-3:
- [ ] Tạo DTOs (Data Transfer Objects)
- [ ] Tạo Repositories (DAL)
- [ ] Tạo Services (BLL)
- [ ] Refactor Controllers
- [ ] Implement Unit of Work

**File cần tạo:**
- `Models/DTOs/*.cs`
- `DAL/Repositories/*.cs`
- `BLL/Services/*.cs`

**Áp dụng kiến thức:**
- Chương 3 - LTCSDL (Kiến trúc đa lớp)
- Chương 2,3,4 - OOP (Class, Inheritance, Interface)

---

### **GIAI ĐOẠN 3: DATABASE & T-SQL (2 tuần)** 🟡 MEDIUM

#### Week 4-5:
- [ ] Tạo Stored Procedures
- [ ] Tạo Triggers
- [ ] Tạo User-Defined Functions
- [ ] Tối ưu LINQ queries
- [ ] Add indexes cho database

**File cần tạo/sửa:**
- `qlbanhang.sql` (thêm SPs, Triggers, Functions)
- Controllers (gọi SPs)

**Áp dụng kiến thức:** Chương 1 - LTCSDL (T-SQL)

---

### **GIAI ĐOẠN 4: WEB API (2 tuần)** 🟢 LOW

#### Week 6-7:
- [ ] Thêm Web API Controllers
- [ ] Implement RESTful endpoints
- [ ] Add JWT authentication
- [ ] API documentation (Swagger)

**File cần tạo:**
- `Controllers/API/*.cs`
- `App_Start/WebApiConfig.cs`

**Áp dụng kiến thức:** Chương 6 - LTCSDL (Web API)

---

### **GIAI ĐOẠN 5: FRONTEND MODERNIZATION (3 tuần)** 🟢 LOW

#### Week 8-10:
- [ ] Upgrade Bootstrap 3 → 5
- [ ] Thêm CSS Variables
- [ ] Refactor CSS (Flexbox/Grid)
- [ ] Modernize JavaScript (ES6+)
- [ ] Add animations

**File cần sửa:**
- `Content/*.css`
- `Views/**/*.cshtml`
- `Scripts/*.js`

**Áp dụng kiến thức:**
- HTML5: Chương 7 (Forms), Chương 8 (Semantic), Chương 12 (Best Practices)
- CSS3: Chương 6 (Flexbox), Chương 7 (Grid), Chương 9 (Animations), Chương 11 (Responsive)
- ES6: Chương 3 (Arrow Functions), Chương 5 (Destructuring), Chương 7 (Classes), Chương 12 (Async/Await)

---

## 📊 BẢNG ĐÁNH GIÁ ĐỒ ÁN THEO ĐỀ CƯƠNG

| Tiêu chí | Điểm tối đa | Điểm hiện tại | Sau cải thiện |
|----------|-------------|---------------|---------------|
| **T-SQL (SP, Triggers, Functions)** | 20 | 5 | 18 |
| **ADO.NET / EF** | 15 | 12 | 14 |
| **Kiến trúc đa lớp** | 20 | 8 | 18 |
| **LINQ** | 10 | 6 | 9 |
| **Repository Pattern** | 10 | 0 | 9 |
| **Web API** | 15 | 0 | 13 |
| **OOP Principles** | 10 | 5 | 9 |
| **TỔNG** | **100** | **36** | **90** |

**Dự đoán điểm:**
- Hiện tại: ~5-6/10 (đủ dùng, nhiều thiếu sót)
- Sau cải thiện: 8.5-9/10 (xuất sắc)

---

## 📚 TÀI LIỆU HỌC TẬP THEO GIAI ĐOẠN

### Giai đoạn 1-2 (Kiến trúc):
- **Đọc:** Chương 3 - LTCSDL
- **Xem:** Repository Pattern, Unit of Work Pattern
- **Làm:** Refactor 1 controller hoàn chỉnh

### Giai đoạn 3 (Database):
- **Đọc:** Chương 1 - LTCSDL
- **Xem:** T-SQL tutorials
- **Làm:** 5-10 Stored Procedures

### Giai đoạn 4 (API):
- **Đọc:** Chương 6 - LTCSDL
- **Xem:** RESTful API design
- **Làm:** 1 bộ CRUD API hoàn chỉnh

### Giai đoạn 5 (Frontend):
- **Đọc HTML5:** Chương 7, 8, 9, 12 (Forms, Semantic, APIs, Best Practices)
- **Đọc CSS3:** Chương 6, 7, 9, 11, 12 (Flexbox, Grid, Animations, Responsive, Advanced)
- **Đọc ES6:** Chương 2, 3, 5, 6, 7, 9, 12 (Let/Const, Arrow Functions, Destructuring, Spread, Classes, Promises, Async/Await)
- **Làm:** Refactor từng trang một (Login → Cart → Home → Products)

---

## ✅ CHECKLIST HOÀN THIỆN

### Chức năng bắt buộc:
- [x] Login/Register
- [x] CRUD Products
- [x] Shopping Cart
- [x] Checkout & Order
- [x] Email notification
- [x] Admin panel
- [x] Role-based authorization

### Chức năng nâng cao (đề xuất thêm):
- [ ] Stored Procedures cho business logic
- [ ] Triggers để audit changes
- [ ] Repository Pattern
- [ ] Unit of Work
- [ ] Web API
- [ ] Search với full-text search
- [ ] Pagination
- [ ] Sorting & Filtering
- [ ] Product reviews
- [ ] Order history
- [ ] Password reset
- [ ] Email verification
- [ ] 2FA (Two-factor authentication)
- [ ] Dashboard với charts
- [ ] Export to Excel/PDF
- [ ] Image optimization
- [ ] Caching (MemoryCache, Redis)

---

## 🎓 KẾT LUẬN

Dựa trên đề cương môn học, project cần tập trung cải thiện:

**1. Lập Trình Cơ Sở Dữ Liệu (LTCSDL):**
- ⭐⭐⭐ Chương 1: T-SQL (SP, Triggers, Functions)
- ⭐⭐⭐ Chương 3: Kiến trúc đa lớp
- ⭐⭐ Chương 4: LINQ optimization
- ⭐⭐ Chương 5: Repository Pattern
- ⭐ Chương 6: Web API (bonus)

**2. Lập Trình Hướng Đối Tượng:**
- ⭐⭐ Chương 2: Refactor classes
- ⭐⭐ Chương 3: Inheritance (BaseController)
- ⭐⭐ Chương 4: Interfaces & Polymorphism

**3. HTML5/CSS3/ES6:**
- HTML5: Chương 8 (Semantic Elements), Chương 12 (Best Practices)
- CSS3: Chương 6 (Flexbox), Chương 7 (Grid), Chương 9 (Animations)
- ES6: Chương 3, 5, 7, 12 (Arrow Functions, Destructuring, Classes, Async/Await)
- ⭐ Modernize frontend (nice to have)

**Ưu tiên thực hiện:** 1 → 2 → 3

**Thời gian ước tính:** 8-10 tuần (2-2.5 tháng)

**Kết quả mong đợi:** Nâng điểm từ 5-6/10 lên 8.5-9/10

---

Bạn muốn bắt đầu từ giai đoạn nào? Tôi có thể hướng dẫn chi tiết từng bước! 😊
