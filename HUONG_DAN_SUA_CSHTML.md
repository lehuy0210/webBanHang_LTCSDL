# 📘 HƯỚNG DẪN SỬA FILE .CSHTML

## 🤔 FILE .CSHTML LÀ GÌ?

**.cshtml** = **C# + HTML** = **Razor View**

Đây là file **View** trong mô hình **MVC** (Model-View-Controller):
- **Model** (C#): Dữ liệu (User, Product, Order...)
- **View** (.cshtml): Giao diện HTML hiển thị cho người dùng
- **Controller** (C#): Xử lý logic, gửi dữ liệu cho View

**Razor** là template engine cho phép viết code C# trong HTML.

---

## 📁 CẤU TRÚC THỨ MỤC VIEWS

```
Views/
├── Shared/              ← Layout chung cho toàn website
│   ├── _Layout.cshtml   ← Template chính (header, footer, menu)
│   ├── LayoutBlank.cshtml
│   └── _LoginPartial.cshtml
├── Home/                ← Views của HomeController
│   ├── Index.cshtml     ← Trang chủ
│   ├── Detail.cshtml    ← Chi tiết sản phẩm
│   └── Contact.cshtml
├── Login/               ← Views của LoginController
│   └── Index.cshtml     ← Trang đăng nhập
├── Register/            ← Views của RegisterController
│   └── Index.cshtml     ← Trang đăng ký
├── Cart/                ← Views của CartController
│   └── Index.cshtml     ← Giỏ hàng
└── Products/            ← Views của ProductsController
    ├── Ao.cshtml
    └── Quan.cshtml
```

**Quy tắc:**
- Controller: `HomeController.cs` → Action: `Index()` → View: `Views/Home/Index.cshtml`
- Controller: `CartController.cs` → Action: `Order()` → View: `Views/Cart/Order.cshtml`

---

## 🔍 PHÂN TÍCH FILE CSHTML CỤ THỂ

### Ví dụ 1: `/Views/Login/Index.cshtml`

```cshtml
@model webBH.Models.User    ← Khai báo Model (object truyền từ Controller)

@{
    Layout = "~/Views/Shared/LayoutBlank.cshtml";  ← Chọn layout (không dùng header/footer chung)
}

@using (Html.BeginForm())   ← Tạo form POST đến LoginController.Index()
{
    @Html.AntiForgeryToken()  ← Token chống CSRF attack

    <h2 style="color: #dfe6e9">Đăng nhập</h2>

    <div class="inputbox">
        <ion-icon name="mail-outline"></ion-icon>
        @Html.TextBoxFor(m => m.email, new { @class = "form-control", placeholder="email" })
        ← Tạo input text, bind với property User.email
    </div>

    <div class="inputbox">
        <ion-icon name="lock-closed-outline"></ion-icon>
        @Html.PasswordFor(m => m.password, new { @class = "form-control", placeholder = "password" })
        ← Tạo input password, bind với property User.password
    </div>

    <button>Đăng nhập</button>  ← Submit button

    <div class="register">
        <p>Không có tài khoản
            @Html.ActionLink("đăng ký tại đây !", "Index", "Register")
            ← Tạo link đến RegisterController.Index()
        </p>
    </div>
}
```

**Giải thích:**
- `@model User` → View này nhận object User từ Controller
- `@Html.TextBoxFor()` → Tạo `<input type="text">` tự động bind với Model
- `@Html.ActionLink()` → Tạo `<a href="">` link đến action khác

---

### Ví dụ 2: `/Views/Cart/Index.cshtml`

```cshtml
@model List<webBH.Models.Cart>   ← Nhận List<Cart> từ CartController
@using webBH.Models

<body>
    <div class="shopping-cart" style="margin-top: 15vh">
        <div class="column-labels">
            <label class="product-image">Image</label>
            <label class="product-details">Product</label>
            <label class="product-price">Giá</label>
            <label class="product-quantity">Số lượng</label>
            <label class="product-removal">Xóa</label>
            <label class="product-line-price">Tổng</label>
        </div>

        @foreach (var item in Model)   ← Lặp qua từng sản phẩm trong giỏ hàng
        {
            <div class="product">
                <div class="product-image">
                    @Html.Image("../" + item.Product.image, "", "")
                    ← Hiển thị ảnh sản phẩm
                </div>

                <div class="product-details">
                    @Html.DisplayFor(modelItem => item.Product.name)
                    ← Hiển thị tên sản phẩm (read-only)
                </div>

                <div class="product-price">
                    @Html.DisplayFor(modelItem => item.Product.price)
                    ← Hiển thị giá
                </div>

                <form action="@Url.Action("Update", "Cart")" method="post">
                    ← Form POST đến CartController.Update()

                    <div class="product-quantity">
                        <input type="hidden" name="id" value="@item.id" />
                        <input type="hidden" name="id_product" value="@item.id_product" />
                        <input type="number" min="1" name="quantity" value="@item.quantity" />
                        ← Input số lượng
                    </div>

                    <div class="product-removal">
                        <input type="submit" value="Cập nhật " class="update-product" />
                        @Html.ActionLink("Xóa", "Delete", new { id = item.id }, new { @class = "remove-product" })
                        ← Link xóa sản phẩm
                    </div>
                </form>

                <div class="product-line-price">
                    @Html.DisplayFor(modelItem => item.total_money) đ
                </div>
            </div>
        }

        <div class="totals">
            <div class="totals-item totals-item-total">
                <label>Tổng tiền</label>
                <div class="totals-value" id="cart-total">@ViewBag.total đ</div>
                ← Hiển thị tổng tiền từ ViewBag (dữ liệu từ Controller)
            </div>
        </div>

        @Html.ActionLink("Thanh toán", "Order", null, new { @class = "checkout" })
        ← Button thanh toán
    </div>
</body>

<script>
    var message = '@TempData["Message"]';  ← Lấy message từ Controller
    if (message) {
        alert(message);  ← Hiển thị alert
    }
</script>
```

**Giải thích:**
- `@model List<Cart>` → View nhận danh sách giỏ hàng
- `@foreach (var item in Model)` → Lặp qua từng item
- `@ViewBag.total` → Dữ liệu động từ Controller
- `@TempData["Message"]` → Message từ Controller (chỉ tồn tại 1 request)

---

### Ví dụ 3: `/Views/Home/Index.cshtml`

```cshtml
@model IEnumerable<webBH.Models.Product>  ← Nhận danh sách Product
@using webBH.Models

@{
    ViewBag.Title = "Index";  ← Set title cho trang
}

@Styles.Render("~/Content/HomeStyle.css")  ← Load CSS

<div class="products" style="margin-top: 3vw">
    @foreach (var item in Model)  ← Lặp qua từng sản phẩm
    {
        <div class="product">
            <div class="image">
                @Html.Image("../" + item.image, "", "")
            </div>

            <h3 class="nameProduct">
                @Html.DisplayFor(m => item.name)  ← Hiển thị tên
            </h3>

            <span class="price">
                @Html.DisplayFor(m => item.price) đ
            </span>

            <div class="footer" style="margin-top: 10px">
                <div class="detail">
                    <button>
                        @Html.ActionLink("Xem chi tiết", "Detail", new { id = item.id })
                        ← Link đến HomeController.Detail(id)
                    </button>
                </div>

                <div class="bay">
                    @if (Session["UserId"] != null)  ← Kiểm tra đăng nhập
                    {
                        @Html.ActionLink("Thêm vào giỏ hàng", "AddToCart", "Cart",
                            new { id_product = item.id, id_user = Session["UserId"] }, null)
                        ← Thêm vào giỏ nếu đã đăng nhập
                    }
                    else
                    {
                        @Html.ActionLink("Thêm vào giỏ hàng", "Index", "Login")
                        ← Chuyển đến login nếu chưa đăng nhập
                    }
                </div>
            </div>
        </div>
    }
</div>
```

---

### Ví dụ 4: `/Views/Shared/_Layout.cshtml` (Template chính)

```cshtml
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewBag.Title - My ASP.NET Application</title>
    ← Title động từ View con

    @Styles.Render("~/Content/css")
    @Scripts.Render("~/bundles/modernizr")
</head>
<body>
    <div class="navbar navbar-inverse navbar-fixed-top">
        <div class="container">
            @Html.ActionLink("OU SHOP", "Index", "Home", new { area = "" }, new { @class = "navbar-brand" })
            ← Logo trang web

            <div class="navbar-collapse collapse">
                <ul class="nav navbar-nav">
                    <li>@Html.ActionLink("Trang chủ", "Index", "Home")</li>
                    <li>@Html.ActionLink("Áo", "Ao", "Products")</li>
                    <li>@Html.ActionLink("Quần", "Quan", "Products")</li>
                    <li>@Html.ActionLink("Sản phẩm khác", "Contact", "Home")</li>

                    <li>
                        @using (Html.BeginForm("Index", "Home", FormMethod.Post))
                        {
                            <div class="input-group mb-3" style="margin-top:7px">
                                @Html.TextBox("SearchString", "", new { @class = "form-control" })
                                <input type="submit" class="btn btn-default" value="Tìm kiếm" />
                            </div>
                        }
                    </li>
                </ul>

                @Html.Partial("_LoginPartial")  ← Load partial view (user menu)
            </div>
        </div>
    </div>

    <div class="container body-content">
        @RenderBody()  ← Đây là chỗ View con được render vào
        ← Ví dụ: Home/Index.cshtml sẽ được insert vào đây

        <hr />
        <footer>
            <p>&copy; @DateTime.Now.Year - Group 17 - LTCSDL - Ho Chi Minh Open University</p>
        </footer>
    </div>

    @Scripts.Render("~/bundles/jquery")
    @Scripts.Render("~/bundles/bootstrap")
    @RenderSection("scripts", required: false)  ← Cho phép View con thêm script riêng
</body>
</html>
```

---

## 🛠️ CÁCH SỬA FILE .CSHTML

### 1. Sửa Text/Nội dung (Dễ nhất)

**Ví dụ: Đổi "Đăng nhập" thành "Sign In"**

```cshtml
<!-- TRƯỚC -->
<h2 style="color: #dfe6e9">Đăng nhập</h2>

<!-- SAU -->
<h2 style="color: #dfe6e9">Sign In</h2>
```

**Ví dụ: Đổi footer**

```cshtml
<!-- File: Views/Shared/_Layout.cshtml -->

<!-- TRƯỚC -->
<footer>
    <p>&copy; @DateTime.Now.Year - Group 17 - LTCSDL - Ho Chi Minh Open University</p>
</footer>

<!-- SAU -->
<footer>
    <p>&copy; @DateTime.Now.Year - Nhóm 17 - Đại học Mở TP.HCM</p>
    <p>Hotline: 0123.456.789 | Email: support@oushop.vn</p>
</footer>
```

---

### 2. Sửa CSS/Style

**Cách 1: Inline style (nhanh nhưng không tốt)**

```cshtml
<!-- TRƯỚC -->
<h2 style="color: #dfe6e9">Đăng nhập</h2>

<!-- SAU -->
<h2 style="color: #ff6b6b; font-size: 32px; text-align: center;">Đăng nhập</h2>
```

**Cách 2: Sửa file CSS riêng (Tốt hơn)**

```cshtml
<!-- File: Views/Login/Index.cshtml -->
<h2 class="login-title">Đăng nhập</h2>

<!-- Thêm CSS vào file: Content/LoginStyle.css -->
.login-title {
    color: #ff6b6b;
    font-size: 32px;
    text-align: center;
    margin-bottom: 30px;
}
```

---

### 3. Thêm/Xóa Trường Input

**Ví dụ: Thêm trường "Số điện thoại" vào form đăng ký**

```cshtml
<!-- File: Views/Register/Index.cshtml -->

<!-- Thêm sau trường email -->
<div class="inputbox">
    <ion-icon name="call-outline"></ion-icon>
    @Html.TextBoxFor(m => m.phone, new { @class = "form-control", placeholder="Số điện thoại" })
</div>
```

**Lưu ý:** Phải sửa Model và Controller tương ứng:
1. Thêm property `phone` vào Model `User.cs`
2. Thêm column `phone` vào database
3. Update Entity Framework model

---

### 4. Sửa Navigation Menu

**Ví dụ: Thêm menu "Giỏ hàng"**

```cshtml
<!-- File: Views/Shared/_Layout.cshtml -->

<ul class="nav navbar-nav">
    <li>@Html.ActionLink("Trang chủ", "Index", "Home")</li>
    <li>@Html.ActionLink("Áo", "Ao", "Products")</li>
    <li>@Html.ActionLink("Quần", "Quan", "Products")</li>

    <!-- THÊM DÒNG NÀY -->
    <li>@Html.ActionLink("Giỏ hàng", "Index", "Cart")</li>

    <li>@Html.ActionLink("Sản phẩm khác", "Contact", "Home")</li>
</ul>
```

---

### 5. Thay đổi Hiển thị Dữ liệu

**Ví dụ: Hiển thị giá có định dạng số**

```cshtml
<!-- TRƯỚC -->
<span class="price">@Html.DisplayFor(m => item.price) đ</span>

<!-- SAU - Thêm định dạng số -->
<span class="price">@string.Format("{0:N0}", item.price) đ</span>
<!-- Output: 500000 → 500,000 đ -->
```

**Ví dụ: Hiển thị ngày theo định dạng Việt Nam**

```cshtml
<!-- TRƯỚC -->
@Html.DisplayFor(m => item.date)

<!-- SAU -->
@item.date.ToString("dd/MM/yyyy HH:mm")
<!-- Output: 12/11/2025 14:30 -->
```

---

### 6. Thêm Điều kiện IF/ELSE

**Ví dụ: Hiển thị badge "Mới" cho sản phẩm trong 7 ngày**

```cshtml
<div class="product">
    @if ((DateTime.Now - item.created_date).TotalDays <= 7)
    {
        <span class="badge badge-danger">MỚI</span>
    }

    <div class="image">
        @Html.Image("../" + item.image, "", "")
    </div>

    <h3>@item.name</h3>
</div>
```

**Ví dụ: Hiển thị "Hết hàng" nếu quantity = 0**

```cshtml
@if (item.quantity > 0)
{
    <button class="btn-add-cart">Thêm vào giỏ</button>
}
else
{
    <button class="btn-out-stock" disabled>Hết hàng</button>
}
```

---

### 7. Sửa Alert Message (TempData)

**Ví dụ: Thay đổi cách hiển thị message**

```cshtml
<!-- TRƯỚC - Alert JavaScript -->
<script>
    var message = '@TempData["Message"]';
    if (message) {
        alert(message);
    }
</script>

<!-- SAU - Bootstrap Toast -->
@if (TempData["Message"] != null)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        <strong>Thông báo:</strong> @TempData["Message"]
        <button type="button" class="close" data-dismiss="alert">
            <span>&times;</span>
        </button>
    </div>
}
```

---

### 8. Thêm Icon (Font Awesome, Ionicons)

```cshtml
<!-- TRƯỚC -->
<button>Thêm vào giỏ hàng</button>

<!-- SAU - Thêm icon -->
<button>
    <i class="fas fa-shopping-cart"></i> Thêm vào giỏ hàng
</button>

<!-- Hoặc dùng Ionicons -->
<button>
    <ion-icon name="cart-outline"></ion-icon> Thêm vào giỏ hàng
</button>
```

**Lưu ý:** Phải thêm CDN vào Layout:

```cshtml
<!-- File: Views/Shared/_Layout.cshtml -->
<head>
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css">

    <!-- Ionicons -->
    <script type="module" src="https://unpkg.com/ionicons@5.5.2/dist/ionicons/ionicons.esm.js"></script>
</head>
```

---

## 🎯 CÁC RAZOR SYNTAX THƯỜNG DÙNG

### 1. Hiển thị dữ liệu

```cshtml
@item.name                           <!-- Hiển thị text -->
@Html.DisplayFor(m => m.name)        <!-- Hiển thị (tự động format) -->
@Html.DisplayNameFor(m => m.name)    <!-- Hiển thị label -->
```

### 2. Tạo input

```cshtml
@Html.TextBoxFor(m => m.email)                     <!-- Input text -->
@Html.PasswordFor(m => m.password)                 <!-- Input password -->
@Html.TextAreaFor(m => m.description)              <!-- Textarea -->
@Html.DropDownListFor(m => m.category_id, items)   <!-- Select dropdown -->
@Html.CheckBoxFor(m => m.is_active)                <!-- Checkbox -->
@Html.RadioButtonFor(m => m.sex, "1")              <!-- Radio button -->
```

### 3. Tạo link/button

```cshtml
<!-- Link đến action -->
@Html.ActionLink("Chi tiết", "Detail", new { id = 5 })
<!-- Output: <a href="/Home/Detail/5">Chi tiết</a> -->

<!-- Link đến controller khác -->
@Html.ActionLink("Đăng nhập", "Index", "Login")
<!-- Output: <a href="/Login">Đăng nhập</a> -->

<!-- Link với CSS class -->
@Html.ActionLink("Xóa", "Delete", new { id = 5 }, new { @class = "btn btn-danger" })
<!-- Output: <a href="/Home/Delete/5" class="btn btn-danger">Xóa</a> -->
```

### 4. Form

```cshtml
<!-- Form POST đến action hiện tại -->
@using (Html.BeginForm())
{
    <!-- Form content -->
}

<!-- Form POST đến action cụ thể -->
@using (Html.BeginForm("Login", "Account", FormMethod.Post))
{
    <!-- Form content -->
}

<!-- Form với HTML thuần -->
<form action="@Url.Action("Update", "Cart")" method="post">
    <input type="text" name="quantity" />
    <button type="submit">Cập nhật</button>
</form>
```

### 5. Điều kiện

```cshtml
@if (condition)
{
    <p>True</p>
}
else
{
    <p>False</p>
}

@if (user.Role == "Admin")
{
    <button>Delete</button>
}
else if (user.Role == "Moderator")
{
    <button>Edit</button>
}
else
{
    <button disabled>View only</button>
}
```

### 6. Vòng lặp

```cshtml
<!-- Foreach -->
@foreach (var item in Model)
{
    <div>@item.name</div>
}

<!-- For -->
@for (int i = 0; i < 10; i++)
{
    <div>Item @i</div>
}

<!-- While -->
@{
    int count = 0;
}
@while (count < 5)
{
    <div>@count</div>
    count++;
}
```

### 7. Session/ViewBag/TempData

```cshtml
<!-- Session -->
@Session["UserId"]
@Session["UserName"]

<!-- ViewBag (dữ liệu từ Controller) -->
@ViewBag.Message
@ViewBag.Total

<!-- TempData (tồn tại 1 request, dùng cho redirect) -->
@TempData["Message"]

<!-- ViewData -->
@ViewData["Title"]
```

### 8. URL Helper

```cshtml
<!-- Generate URL -->
@Url.Action("Index", "Home")
<!-- Output: /Home/Index -->

<!-- Generate URL với parameters -->
@Url.Action("Detail", "Products", new { id = 5 })
<!-- Output: /Products/Detail/5 -->

<!-- Get current URL -->
@Request.Url
@Request.RawUrl
```

---

## 🔥 VÍ DỤ THỰC TẾ: SỬA TRANG GIỎ HÀNG

### Yêu cầu: Thêm tính năng "Xóa tất cả" và hiển thị số lượng sản phẩm

**File: `/Views/Cart/Index.cshtml`**

```cshtml
@model List<webBH.Models.Cart>

<body>
    <div class="shopping-cart" style="margin-top: 15vh">

        <!-- THÊM: Header với số lượng sản phẩm -->
        <div class="cart-header">
            <h2>Giỏ hàng của bạn (@Model.Count sản phẩm)</h2>

            @if (Model.Count > 0)
            {
                @Html.ActionLink("Xóa tất cả", "ClearCart", null,
                    new {
                        @class = "btn btn-danger",
                        onclick = "return confirm('Bạn có chắc muốn xóa tất cả sản phẩm?');"
                    })
            }
        </div>

        <!-- Kiểm tra giỏ hàng có rỗng không -->
        @if (Model.Count == 0)
        {
            <div class="empty-cart">
                <img src="~/Images/empty-cart.png" alt="Empty cart" />
                <h3>Giỏ hàng của bạn đang trống</h3>
                @Html.ActionLink("Tiếp tục mua sắm", "Index", "Home", null, new { @class = "btn btn-primary" })
            </div>
        }
        else
        {
            <div class="column-labels">
                <label class="product-image">Image</label>
                <label class="product-details">Product</label>
                <label class="product-price">Giá</label>
                <label class="product-quantity">Số lượng</label>
                <label class="product-removal">Xóa</label>
                <label class="product-line-price">Tổng</label>
            </div>

            @foreach (var item in Model)
            {
                <div class="product">
                    <div class="product-image">
                        @Html.Image("../" + item.Product.image, "", "")
                    </div>

                    <div class="product-details">
                        <h3>@item.Product.name</h3>
                        <p class="product-description">Size: @item.Product.size | Màu: @item.Product.color</p>
                    </div>

                    <div class="product-price">
                        @string.Format("{0:N0}", item.Product.price) đ
                    </div>

                    <form action="@Url.Action("Update", "Cart")" method="post">
                        <div class="product-quantity">
                            <input type="hidden" name="id" value="@item.id" />
                            <input type="hidden" name="id_product" value="@item.id_product" />
                            <input type="number" min="1" max="99" name="quantity" value="@item.quantity" />
                        </div>

                        <div class="product-removal">
                            <input type="submit" value="Cập nhật" class="update-product btn btn-sm btn-info" />
                            @Html.ActionLink("Xóa", "Delete", new { id = item.id },
                                new {
                                    @class = "remove-product btn btn-sm btn-danger",
                                    onclick = "return confirm('Xóa sản phẩm này?');"
                                })
                        </div>
                    </form>

                    <div class="product-line-price">
                        @string.Format("{0:N0}", item.total_money) đ
                    </div>
                </div>
            }

            <div class="totals">
                <div class="totals-item">
                    <label>Tạm tính</label>
                    <div class="totals-value">@string.Format("{0:N0}", ViewBag.total) đ</div>
                </div>

                <div class="totals-item">
                    <label>Phí vận chuyển</label>
                    <div class="totals-value">30,000 đ</div>
                </div>

                <div class="totals-item totals-item-total">
                    <label>Tổng cộng</label>
                    <div class="totals-value" id="cart-total">
                        @string.Format("{0:N0}", (int)ViewBag.total + 30000) đ
                    </div>
                </div>
            </div>

            <div class="cart-actions">
                @Html.ActionLink("Tiếp tục mua hàng", "Index", "Home", null, new { @class = "btn btn-secondary" })
                @Html.ActionLink("Thanh toán", "Order", null, new { @class = "checkout btn btn-success btn-lg" })
            </div>
        }
    </div>
</body>

<!-- Alert message với animation -->
@if (TempData["Message"] != null)
{
    <div class="alert alert-success alert-dismissible fade show position-fixed"
         style="top: 20px; right: 20px; z-index: 9999;" role="alert">
        <strong>Thông báo:</strong> @TempData["Message"]
        <button type="button" class="close" data-dismiss="alert">
            <span>&times;</span>
        </button>
    </div>

    <script>
        // Auto hide sau 3 giây
        setTimeout(function() {
            $('.alert').fadeOut('slow');
        }, 3000);
    </script>
}
```

---

## 🚀 THỰC HÀNH: SỬA TRANG LOGIN

### Yêu cầu: Thêm "Remember Me" và "Forgot Password"

**File: `/Views/Login/Index.cshtml`**

```cshtml
@model webBH.Models.User

@{
    Layout = "~/Views/Shared/LayoutBlank.cshtml";
}

@using (Html.BeginForm())
{
    @Html.AntiForgeryToken()

    <section>
        <div class="form-box">
            <div class="form-value">
                <form action="">
                    <h2 style="color: #dfe6e9">Đăng nhập</h2>

                    <!-- Hiển thị error message -->
                    @if (!string.IsNullOrEmpty(ViewBag.ErrorMessage))
                    {
                        <div class="alert alert-danger">
                            @ViewBag.ErrorMessage
                        </div>
                    }

                    <div class="inputbox">
                        <ion-icon name="mail-outline"></ion-icon>
                        @Html.TextBoxFor(m => m.email, new {
                            @class = "form-control",
                            placeholder = "Email",
                            required = "required",
                            type = "email"
                        })
                        @Html.ValidationMessageFor(m => m.email, "", new { @class = "text-danger" })
                    </div>

                    <div class="inputbox">
                        <ion-icon name="lock-closed-outline"></ion-icon>
                        @Html.PasswordFor(m => m.password, new {
                            @class = "form-control",
                            placeholder = "Password",
                            autocomplete = "off",
                            required = "required",
                            minlength = "6"
                        })
                        @Html.ValidationMessageFor(m => m.password, "", new { @class = "text-danger" })
                    </div>

                    <div class="forget">
                        <label>
                            <input type="checkbox" name="rememberMe"> Ghi nhớ đăng nhập
                        </label>
                        <a href="@Url.Action("ForgotPassword", "Account")" style="color: #fff;">
                            Quên mật khẩu?
                        </a>
                    </div>

                    <button type="submit">Đăng nhập</button>

                    <!-- Divider -->
                    <div class="divider">
                        <span>Hoặc</span>
                    </div>

                    <!-- Social login (optional) -->
                    <div class="social-login">
                        <button type="button" class="btn-google">
                            <i class="fab fa-google"></i> Google
                        </button>
                        <button type="button" class="btn-facebook">
                            <i class="fab fa-facebook-f"></i> Facebook
                        </button>
                    </div>

                    <div class="register">
                        <p>
                            Chưa có tài khoản?
                            @Html.ActionLink("Đăng ký ngay!", "Index", "Register", null, new { @class = "link-primary" })
                        </p>
                    </div>
                </form>
            </div>
        </div>
    </section>

    <script type="module" src="https://unpkg.com/ionicons@5.5.2/dist/ionicons/ionicons.esm.js"></script>
    <script nomodule src="https://unpkg.com/ionicons@5.5.2/dist/ionicons/ionicons.js"></script>
}

@section scripts {
    @Scripts.Render("~/bundles/jqueryval")
}
```

---

## 📝 CHECKLIST KHI SỬA CSHTML

- [ ] **Backup file gốc** trước khi sửa (copy thành `Index.cshtml.bak`)
- [ ] **Kiểm tra syntax** - Razor rất strict về cú pháp
- [ ] **Test trên local** trước khi deploy
- [ ] **Kiểm tra responsive** - Test trên mobile/tablet
- [ ] **Validate HTML** - Đảm bảo không lỗi tag đóng/mở
- [ ] **Check encoding** - File phải UTF-8 để hiển thị tiếng Việt
- [ ] **Clear browser cache** khi test

---

## ⚠️ LƯU Ý QUAN TRỌNG

### 1. Encoding tiếng Việt
```cshtml
<!-- Thêm vào <head> -->
<meta charset="utf-8" />
```

### 2. Anti-forgery token (bảo mật)
```cshtml
<!-- LUÔN thêm vào form POST -->
@using (Html.BeginForm())
{
    @Html.AntiForgeryToken()
    <!-- form content -->
}
```

### 3. HTML Encode để tránh XSS
```cshtml
<!-- KHÔNG AN TOÀN -->
@Html.Raw(Model.Description)  <!-- Hiển thị HTML thuần -->

<!-- AN TOÀN -->
@Model.Description  <!-- Tự động encode HTML -->
```

### 4. Null checking
```cshtml
<!-- Kiểm tra null trước khi dùng -->
@if (Model != null && Model.Count > 0)
{
    @foreach (var item in Model)
    {
        <div>@item.name</div>
    }
}

<!-- Hoặc dùng null coalescing -->
@(ViewBag.Message ?? "Không có thông báo")
```

---

## 🎓 TÀI LIỆU THAM KHẢO

1. **Razor Syntax:** https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor
2. **HTML Helpers:** https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/views/creating-custom-html-helpers-cs
3. **Bootstrap 4:** https://getbootstrap.com/docs/4.6/
4. **Font Awesome:** https://fontawesome.com/
5. **Ionicons:** https://ionic.io/ionicons

---

## 💡 MẸO HAY

1. **Sử dụng ReSharper hoặc Visual Studio Intellisense** - Tự động gợi ý code
2. **Dùng snippet** - Gõ `for` + Tab → tự động tạo vòng lặp
3. **Ctrl + K, Ctrl + D** trong Visual Studio → Format code tự động
4. **Ctrl + F5** → Run without debugging (nhanh hơn)
5. **Browser DevTools (F12)** → Debug HTML/CSS real-time

---

Có thắc mắc gì cứ hỏi tôi nhé! 😊
