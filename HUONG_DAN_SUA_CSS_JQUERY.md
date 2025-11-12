# 📘 HƯỚNG DẪN SỬA CSS BOOTSTRAP & JQUERY

## 📁 CẤU TRÚC THỨ MỤC CSS/JS TRONG PROJECT

```
webBH/
├── Content/                          ← Chứa tất cả CSS
│   ├── bootstrap.css                 ← Bootstrap CSS (146KB)
│   ├── bootstrap.min.css             ← Bootstrap minified (121KB)
│   ├── bootstrap-theme.css           ← Bootstrap theme
│   ├── bootstrap-theme.min.css
│   ├── Site.css                      ← CSS CHUNG cho toàn website
│   ├── LoginStyle.css                ← CSS riêng cho trang Login
│   ├── RegisterStyle.css             ← CSS riêng cho trang Register
│   ├── CartStyle.css                 ← CSS riêng cho giỏ hàng
│   ├── HomeStyle.css                 ← CSS riêng cho trang chủ
│   ├── DetailStyle.css               ← CSS riêng cho chi tiết sản phẩm
│   ├── PagedList.css                 ← CSS cho phân trang
│   └── all.min.css                   ← Font Awesome icons
│
├── Scripts/                          ← Chứa tất cả JavaScript
│   ├── jquery-3.4.1.js               ← jQuery 3.4.1 (280KB)
│   ├── jquery-3.4.1.min.js           ← jQuery minified (88KB)
│   ├── bootstrap.js                  ← Bootstrap JS (75KB)
│   ├── bootstrap.min.js              ← Bootstrap minified (40KB)
│   ├── jquery.validate.js            ← jQuery Validate plugin
│   ├── jquery.validate.min.js
│   ├── jquery.validate.unobtrusive.js
│   └── modernizr-2.8.3.js            ← Feature detection
│
└── App_Start/
    └── BundleConfig.cs               ← Cấu hình bundle CSS/JS
```

---

## 🎨 PHẦN 1: SỬA CSS BOOTSTRAP

### 1.1. HIỂU CƠ CHẾ HOẠT ĐỘNG

**Bootstrap trong project này:**
- File gốc: `Content/bootstrap.css` (146KB - không minify, có comment)
- File production: `Content/bootstrap.min.css` (121KB - minified, không có comment)

**Quy tắc load CSS trong project:**

```cshtml
<!-- File: Views/Shared/_Layout.cshtml -->
@Styles.Render("~/Content/css")

<!-- Render ra HTML: -->
<link href="/Content/bootstrap.css" rel="stylesheet"/>
<link href="/Content/Site.css" rel="stylesheet"/>
```

**Thứ tự ưu tiên CSS:**
1. `bootstrap.css` (load đầu tiên)
2. `Site.css` (load sau, override được bootstrap)
3. CSS inline trong .cshtml (ưu tiên cao nhất)

---

### 1.2. CÁCH SỬA: OVERRIDE BOOTSTRAP CSS (KHUYẾN NGHỊ)

**⚠️ KHÔNG NÊN sửa trực tiếp file `bootstrap.css`** vì:
- Khi update Bootstrap, mất hết sửa đổi
- Khó maintain
- Không biết đâu là code gốc, đâu là code custom

**✅ NÊN tạo CSS riêng để override**

#### Cách 1: Sửa trong `Site.css` (cho CSS chung toàn website)

**Ví dụ: Đổi màu button primary từ xanh dương → đỏ**

```css
/* File: Content/Site.css */

/* Bootstrap default: #337ab7 (xanh dương) */
/* Override thành màu đỏ */
.btn-primary {
    background-color: #e74c3c !important;
    border-color: #c0392b !important;
}

.btn-primary:hover {
    background-color: #c0392b !important;
    border-color: #a93226 !important;
}
```

**Ví dụ: Đổi font chữ toàn website**

```css
/* File: Content/Site.css */

body {
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif !important;
    font-size: 16px !important;
}
```

**Ví dụ: Tăng border-radius cho tất cả button**

```css
/* File: Content/Site.css */

.btn {
    border-radius: 25px !important; /* Bootstrap default: 4px */
}
```

---

#### Cách 2: Tạo file CSS riêng (cho component cụ thể)

**Ví dụ: Custom CSS cho navbar**

**Bước 1: Tạo file mới**
```
Content/CustomNavbar.css
```

**Bước 2: Viết CSS override**

```css
/* File: Content/CustomNavbar.css */

/* Đổi màu nền navbar từ đen → gradient */
.navbar-inverse {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
    border: none !important;
}

/* Đổi màu text navbar */
.navbar-inverse .navbar-nav > li > a {
    color: #ffffff !important;
    font-weight: 500 !important;
}

.navbar-inverse .navbar-nav > li > a:hover {
    background-color: rgba(255, 255, 255, 0.1) !important;
    border-radius: 5px !important;
}

/* Đổi màu brand (logo) */
.navbar-inverse .navbar-brand {
    color: #ffffff !important;
    font-size: 28px !important;
    font-weight: bold !important;
}
```

**Bước 3: Thêm vào BundleConfig.cs**

```csharp
// File: App_Start/BundleConfig.cs

bundles.Add(new StyleBundle("~/Content/css").Include(
    "~/Content/bootstrap.css",
    "~/Content/site.css",
    "~/Content/CustomNavbar.css"  // ← Thêm dòng này
));
```

**Hoặc load trực tiếp trong View:**

```cshtml
<!-- File: Views/Shared/_Layout.cshtml -->
<head>
    @Styles.Render("~/Content/css")
    <link href="~/Content/CustomNavbar.css" rel="stylesheet" />
</head>
```

---

### 1.3. SỬA TRỰC TIẾP BOOTSTRAP (KHÔNG KHUYẾN NGHỊ)

**Chỉ dùng khi:**
- Cần thay đổi toàn bộ theme Bootstrap
- Biết chắc chắn mình đang làm gì

**Cách làm:**

1. **Backup file gốc:**
```bash
cp Content/bootstrap.css Content/bootstrap.css.backup
```

2. **Mở `Content/bootstrap.css` và tìm kiếm:**

```css
/* Line 2428: Button primary color */
.btn-primary {
  color: #fff;
  background-color: #337ab7;  /* ← Đổi màu này */
  border-color: #2e6da4;      /* ← Và màu này */
}

.btn-primary:hover {
  color: #fff;
  background-color: #286090;  /* ← Đổi màu hover */
  border-color: #204d74;
}
```

3. **Rebuild project** để CSS được compile lại

---

### 1.4. VÍ DỤ THỰC TẾ: CUSTOM BOOTSTRAP TRONG PROJECT

#### Ví dụ 1: Đổi màu tất cả button trong website

```css
/* File: Content/Site.css */

/* Primary button - Màu xanh lá */
.btn-primary {
    background-color: #27ae60 !important;
    border-color: #229954 !important;
}

.btn-primary:hover {
    background-color: #229954 !important;
    border-color: #1e8449 !important;
}

/* Success button - Màu xanh dương */
.btn-success {
    background-color: #3498db !important;
    border-color: #2980b9 !important;
}

/* Danger button - Màu đỏ đậm hơn */
.btn-danger {
    background-color: #e74c3c !important;
    border-color: #c0392b !important;
}

/* Info button - Màu tím */
.btn-info {
    background-color: #9b59b6 !important;
    border-color: #8e44ad !important;
}

/* Thêm shadow cho tất cả button */
.btn {
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1) !important;
    transition: all 0.3s ease !important;
}

.btn:hover {
    transform: translateY(-2px) !important;
    box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15) !important;
}
```

---

#### Ví dụ 2: Custom form input

```css
/* File: Content/Site.css */

/* Override Bootstrap form-control */
.form-control {
    border-radius: 8px !important;
    border: 2px solid #e0e0e0 !important;
    padding: 12px 15px !important;
    transition: all 0.3s ease !important;
}

.form-control:focus {
    border-color: #3498db !important;
    box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1) !important;
}

/* Placeholder style */
.form-control::placeholder {
    color: #95a5a6 !important;
    font-style: italic !important;
}
```

---

#### Ví dụ 3: Custom card/panel

```css
/* File: Content/Site.css */

/* Tạo card đẹp hơn */
.panel {
    border-radius: 12px !important;
    border: none !important;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1) !important;
}

.panel-heading {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
    color: white !important;
    border-radius: 12px 12px 0 0 !important;
    padding: 20px !important;
}

.panel-body {
    padding: 25px !important;
}
```

---

#### Ví dụ 4: Responsive container width

```css
/* File: Content/Site.css */

/* Mở rộng container trên màn hình lớn */
@media (min-width: 1200px) {
    .container {
        max-width: 1400px !important;
    }
}

/* Hiện tại trong _Layout.cshtml có style="width:80%"
   Nếu muốn đổi thành 90% */
.navbar .container,
.body-content {
    width: 90% !important;
}
```

---

### 1.5. CUSTOM THEO TRANG CỤ THỂ

Project này đã có CSS riêng cho từng trang. Đây là cách tốt!

#### Sửa CSS trang Login

**File: `Content/LoginStyle.css`**

```css
/* ĐÃ CÓ trong file - Có thể sửa */

/* Đổi màu background */
section {
    background: url('/Images/background-login.jpg') no-repeat;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); /* ← Đổi thành gradient */
}

/* Đổi màu form box */
.form-box {
    background: rgba(255, 255, 255, 0.95); /* ← Đổi từ transparent thành trắng mờ */
    border: none; /* ← Bỏ viền */
}

/* Đổi màu chữ tiêu đề */
h2 {
    color: #2c3e50 !important; /* ← Đổi từ trắng thành đen */
}

/* Đổi màu input */
.inputbox input {
    color: #2c3e50 !important; /* ← Đổi từ trắng thành đen */
    border-bottom: 2px solid #3498db !important; /* ← Đổi màu underline */
}

/* Đổi màu button */
button {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
    color: white !important;
    font-weight: bold !important;
    transition: all 0.3s ease !important;
}

button:hover {
    transform: scale(1.05) !important;
    box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4) !important;
}
```

---

#### Sửa CSS giỏ hàng

**File: `Content/CartStyle.css`**

```css
/* THÊM VÀO cuối file */

/* Đổi màu button "Cập nhật" */
.update-product {
    background-color: #3498db !important; /* Đổi từ #16a085 → xanh dương */
}

.update-product:hover {
    background-color: #2980b9 !important;
}

/* Đổi màu button "Xóa" */
.remove-product {
    background-color: #e74c3c !important; /* Đổi từ #c66 → đỏ đậm hơn */
}

.remove-product:hover {
    background-color: #c0392b !important;
}

/* Đổi màu button "Thanh toán" */
.checkout {
    background: linear-gradient(135deg, #27ae60 0%, #2ecc71 100%) !important;
    font-size: 20px !important;
    padding: 12px 40px !important;
    border-radius: 50px !important;
    box-shadow: 0 5px 15px rgba(39, 174, 96, 0.3) !important;
    transition: all 0.3s ease !important;
}

.checkout:hover {
    transform: translateY(-3px) !important;
    box-shadow: 0 8px 20px rgba(39, 174, 96, 0.4) !important;
}

/* Thêm animation cho product items */
.product {
    transition: all 0.3s ease !important;
}

.product:hover {
    background-color: #f8f9fa !important;
    transform: translateX(5px) !important;
}
```

---

### 1.6. LOAD CSS TRONG CSHTML

Có 3 cách load CSS vào View:

#### Cách 1: Load qua Bundle (khuyến nghị cho production)

```cshtml
<!-- File: Views/Shared/_Layout.cshtml -->
@Styles.Render("~/Content/css")
<!-- Render ra: bootstrap.css + Site.css -->
```

#### Cách 2: Load trực tiếp (khuyến nghị cho CSS riêng của từng trang)

```cshtml
<!-- File: Views/Login/Index.cshtml -->
@Styles.Render("~/Content/LoginStyle.css")

<!-- Hoặc -->
<link href="~/Content/LoginStyle.css" rel="stylesheet" />
```

**Ví dụ trong project:**

```cshtml
<!-- File: Views/Cart/Index.cshtml dòng 9 -->
@Styles.Render("~/Content/CartStyle.css")

<!-- File: Views/Home/Index.cshtml dòng 8 -->
@Styles.Render("~/Content/HomeStyle.css")
@Styles.Render("~/Content/all.min.css")
```

#### Cách 3: Inline CSS (chỉ dùng cho test nhanh)

```cshtml
<style>
    .my-custom-class {
        color: red;
        font-size: 20px;
    }
</style>
```

---

## ⚙️ PHẦN 2: SỬA/CUSTOM JQUERY

### 2.1. HIỂU CƠ CHẾ HOẠT ĐỘNG

**jQuery trong project:**
- Version: 3.4.1
- File gốc: `Scripts/jquery-3.4.1.js` (280KB)
- File minified: `Scripts/jquery-3.4.1.min.js` (88KB)
- Slim version: `Scripts/jquery-3.4.1.slim.js` (không có AJAX)

**Load jQuery trong project:**

```cshtml
<!-- File: Views/Shared/_Layout.cshtml (dòng 55) -->
@Scripts.Render("~/bundles/jquery")

<!-- Render ra: -->
<script src="/Scripts/jquery-3.4.1.js"></script>
```

---

### 2.2. CÁCH SỬA/THÊM JQUERY CODE

#### ⚠️ KHÔNG BAO GIỜ sửa file `jquery-3.4.1.js`

Tạo file JavaScript riêng để viết code jQuery.

---

#### Cách 1: Viết jQuery trực tiếp trong .cshtml

**Ví dụ: Alert message có trong project**

```cshtml
<!-- File: Views/Cart/Index.cshtml (dòng 71-76) -->
<script>
    var message = '@TempData["Message"]';
    if (message) {
        alert(message);
    }
</script>
```

**Ví dụ: Thêm confirm trước khi xóa**

```cshtml
<!-- File: Views/Cart/Index.cshtml -->
<script>
    $(document).ready(function() {
        // Confirm trước khi xóa sản phẩm
        $('.remove-product').on('click', function(e) {
            if (!confirm('Bạn có chắc muốn xóa sản phẩm này?')) {
                e.preventDefault(); // Hủy action
            }
        });

        // Confirm trước khi thanh toán
        $('.checkout').on('click', function(e) {
            if (!confirm('Xác nhận đặt hàng?')) {
                e.preventDefault();
            }
        });
    });
</script>
```

---

#### Cách 2: Tạo file JavaScript riêng (Khuyến nghị)

**Bước 1: Tạo file mới**
```
Scripts/custom.js
```

**Bước 2: Viết code jQuery**

```javascript
// File: Scripts/custom.js

$(document).ready(function() {
    console.log('Custom JS loaded!');

    // ========== CART PAGE ==========

    // Confirm trước khi xóa
    $('.remove-product').on('click', function(e) {
        if (!confirm('Xóa sản phẩm này khỏi giỏ hàng?')) {
            e.preventDefault();
            return false;
        }
    });

    // Auto update khi thay đổi số lượng
    $('.product-quantity input[type="number"]').on('change', function() {
        $(this).closest('form').submit();
    });

    // Animate khi hover product
    $('.product').hover(
        function() {
            $(this).css('background-color', '#f8f9fa');
        },
        function() {
            $(this).css('background-color', 'transparent');
        }
    );

    // ========== FORM VALIDATION ==========

    // Validate email format
    $('input[type="email"]').on('blur', function() {
        var email = $(this).val();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!emailRegex.test(email) && email !== '') {
            alert('Email không đúng định dạng!');
            $(this).focus();
        }
    });

    // Validate password length
    $('input[type="password"]').on('blur', function() {
        var password = $(this).val();

        if (password.length < 6 && password !== '') {
            alert('Mật khẩu phải có ít nhất 6 ký tự!');
            $(this).focus();
        }
    });

    // ========== PRODUCT LIST ==========

    // Loading animation khi thêm vào giỏ
    $('.bay a').on('click', function(e) {
        var $btn = $(this);
        var originalText = $btn.text();

        $btn.text('Đang thêm...');
        $btn.css('pointer-events', 'none');

        // Sau 1 giây restore
        setTimeout(function() {
            $btn.text(originalText);
            $btn.css('pointer-events', 'auto');
        }, 1000);
    });

    // ========== ALERT AUTO HIDE ==========

    // Auto hide alert sau 3 giây
    if ($('.alert').length > 0) {
        setTimeout(function() {
            $('.alert').fadeOut('slow');
        }, 3000);
    }

    // ========== SMOOTH SCROLL ==========

    $('a[href^="#"]').on('click', function(e) {
        e.preventDefault();
        var target = $(this.hash);

        if (target.length) {
            $('html, body').animate({
                scrollTop: target.offset().top - 70
            }, 800);
        }
    });

    // ========== NAVBAR SCROLL EFFECT ==========

    $(window).scroll(function() {
        if ($(this).scrollTop() > 50) {
            $('.navbar').addClass('navbar-scrolled');
        } else {
            $('.navbar').removeClass('navbar-scrolled');
        }
    });
});
```

**Bước 3: Load file JavaScript**

**Cách 3a: Thêm vào BundleConfig.cs**

```csharp
// File: App_Start/BundleConfig.cs

bundles.Add(new ScriptBundle("~/bundles/custom").Include(
    "~/Scripts/custom.js"
));
```

Sau đó load trong Layout:

```cshtml
<!-- File: Views/Shared/_Layout.cshtml -->
@Scripts.Render("~/bundles/jquery")
@Scripts.Render("~/bundles/bootstrap")
@Scripts.Render("~/bundles/custom")  <!-- ← Thêm dòng này -->
```

**Cách 3b: Load trực tiếp**

```cshtml
<!-- File: Views/Shared/_Layout.cshtml -->
<script src="~/Scripts/custom.js"></script>
```

---

### 2.3. VÍ DỤ THỰC TẾ: JQUERY TRONG PROJECT

#### Ví dụ 1: Cải thiện trang giỏ hàng

**Yêu cầu:**
- Auto update khi đổi số lượng (không cần bấm nút)
- Hiển thị loading spinner
- Tính tổng tiền real-time

```javascript
// File: Scripts/cart.js

$(document).ready(function() {
    // Auto update quantity
    $('.product-quantity input[type="number"]').on('change', function() {
        var $input = $(this);
        var $form = $input.closest('form');
        var $product = $input.closest('.product');

        // Show loading
        $product.css('opacity', '0.5');

        // Submit form
        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: $form.serialize(),
            success: function(response) {
                // Reload page to update totals
                location.reload();
            },
            error: function() {
                alert('Có lỗi xảy ra!');
                $product.css('opacity', '1');
            }
        });
    });

    // Calculate total real-time (without AJAX)
    function updateTotal() {
        var total = 0;

        $('.product').each(function() {
            var price = parseInt($(this).find('.product-price').text().replace(/[^\d]/g, ''));
            var quantity = parseInt($(this).find('.product-quantity input').val());
            var lineTotal = price * quantity;

            // Update line total
            $(this).find('.product-line-price').text(lineTotal.toLocaleString('vi-VN') + ' đ');

            total += lineTotal;
        });

        // Update grand total
        $('#cart-total').text(total.toLocaleString('vi-VN') + ' đ');
    }

    // Update on quantity change (instant)
    $('.product-quantity input[type="number"]').on('input', function() {
        updateTotal();
    });

    // Delete confirmation with sweet animation
    $('.remove-product').on('click', function(e) {
        e.preventDefault();
        var $link = $(this);
        var $product = $link.closest('.product');

        if (confirm('Xóa sản phẩm này?')) {
            // Animate out
            $product.fadeOut('slow', function() {
                // Navigate to delete URL
                window.location.href = $link.attr('href');
            });
        }
    });
});
```

**Load trong Cart/Index.cshtml:**

```cshtml
<!-- File: Views/Cart/Index.cshtml -->
@section scripts {
    <script src="~/Scripts/cart.js"></script>
}
```

---

#### Ví dụ 2: Form validation cho Login

```javascript
// File: Scripts/login-validation.js

$(document).ready(function() {
    $('form').on('submit', function(e) {
        var email = $('input[name="email"]').val();
        var password = $('input[name="password"]').val();
        var hasError = false;

        // Clear previous errors
        $('.error-message').remove();

        // Validate email
        if (email.trim() === '') {
            showError('email', 'Vui lòng nhập email');
            hasError = true;
        } else if (!isValidEmail(email)) {
            showError('email', 'Email không đúng định dạng');
            hasError = true;
        }

        // Validate password
        if (password.trim() === '') {
            showError('password', 'Vui lòng nhập mật khẩu');
            hasError = true;
        } else if (password.length < 6) {
            showError('password', 'Mật khẩu phải có ít nhất 6 ký tự');
            hasError = true;
        }

        if (hasError) {
            e.preventDefault();
            return false;
        }
    });

    function showError(fieldName, message) {
        var $field = $('input[name="' + fieldName + '"]');
        $field.addClass('input-error');
        $field.after('<div class="error-message" style="color: red; font-size: 12px; margin-top: 5px;">' + message + '</div>');
    }

    function isValidEmail(email) {
        var regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    }

    // Remove error on focus
    $('input').on('focus', function() {
        $(this).removeClass('input-error');
        $(this).next('.error-message').remove();
    });
});
```

---

#### Ví dụ 3: Search suggestions (autocomplete)

```javascript
// File: Scripts/search-autocomplete.js

$(document).ready(function() {
    var $searchBox = $('input[name="SearchString"]');
    var $resultsContainer = $('<div class="search-results"></div>');

    $searchBox.after($resultsContainer);

    $searchBox.on('keyup', function() {
        var query = $(this).val();

        if (query.length < 2) {
            $resultsContainer.hide();
            return;
        }

        // AJAX call to search products
        $.ajax({
            url: '/Home/SearchProducts',
            type: 'GET',
            data: { query: query },
            success: function(products) {
                displayResults(products);
            }
        });
    });

    function displayResults(products) {
        $resultsContainer.empty();

        if (products.length === 0) {
            $resultsContainer.html('<div class="no-results">Không tìm thấy sản phẩm</div>');
        } else {
            products.forEach(function(product) {
                var $item = $('<div class="search-item">' +
                    '<img src="' + product.image + '" alt="" width="40">' +
                    '<span>' + product.name + '</span>' +
                    '<span class="price">' + product.price + ' đ</span>' +
                    '</div>');

                $item.on('click', function() {
                    window.location.href = '/Home/Detail/' + product.id;
                });

                $resultsContainer.append($item);
            });
        }

        $resultsContainer.show();
    }

    // Hide results when click outside
    $(document).on('click', function(e) {
        if (!$(e.target).closest('.search-results, input[name="SearchString"]').length) {
            $resultsContainer.hide();
        }
    });
});
```

**CSS cho search suggestions:**

```css
/* File: Content/Site.css */

.search-results {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    background: white;
    border: 1px solid #ddd;
    border-radius: 4px;
    max-height: 400px;
    overflow-y: auto;
    z-index: 1000;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.search-item {
    display: flex;
    align-items: center;
    padding: 10px;
    cursor: pointer;
    gap: 10px;
}

.search-item:hover {
    background-color: #f5f5f5;
}

.search-item img {
    border-radius: 4px;
}

.search-item .price {
    margin-left: auto;
    color: #e74c3c;
    font-weight: bold;
}

.no-results {
    padding: 20px;
    text-align: center;
    color: #999;
}
```

---

#### Ví dụ 4: Image lazy loading

```javascript
// File: Scripts/lazy-load.js

$(document).ready(function() {
    // Lazy load images
    $('img').each(function() {
        var $img = $(this);
        var src = $img.attr('src');

        // Placeholder image
        $img.attr('src', 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"%3E%3Crect fill="%23ddd" width="100" height="100"/%3E%3C/svg%3E');
        $img.attr('data-src', src);
    });

    // Load images when in viewport
    function lazyLoad() {
        $('img[data-src]').each(function() {
            var $img = $(this);

            if (isInViewport($img)) {
                $img.attr('src', $img.attr('data-src'));
                $img.removeAttr('data-src');
                $img.addClass('loaded');
            }
        });
    }

    function isInViewport($el) {
        var elementTop = $el.offset().top;
        var elementBottom = elementTop + $el.outerHeight();
        var viewportTop = $(window).scrollTop();
        var viewportBottom = viewportTop + $(window).height();

        return elementBottom > viewportTop && elementTop < viewportBottom;
    }

    // Check on scroll and resize
    $(window).on('scroll resize', lazyLoad);

    // Initial check
    lazyLoad();
});
```

---

### 2.4. JQUERY PLUGINS PHỔ BIẾN

Project này đã có:
- ✅ jQuery Validate
- ✅ jQuery Unobtrusive Validation

**Thêm plugins khác:**

#### Plugin 1: SweetAlert2 (Alert đẹp hơn)

```html
<!-- Thêm vào _Layout.cshtml -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css">
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
```

**Sử dụng:**

```javascript
// Thay vì alert() thường
Swal.fire({
    title: 'Thành công!',
    text: 'Đã thêm vào giỏ hàng',
    icon: 'success',
    confirmButtonText: 'OK'
});

// Confirm dialog
Swal.fire({
    title: 'Bạn có chắc?',
    text: "Sản phẩm sẽ bị xóa khỏi giỏ hàng!",
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Xóa',
    cancelButtonText: 'Hủy'
}).then((result) => {
    if (result.isConfirmed) {
        // Xóa sản phẩm
        window.location.href = deleteUrl;
    }
});
```

---

#### Plugin 2: Toastr (Toast notifications)

```html
<!-- Thêm vào _Layout.cshtml -->
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.css">
<script src="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.js"></script>
```

**Sử dụng:**

```javascript
// Success toast
toastr.success('Đã thêm vào giỏ hàng!');

// Error toast
toastr.error('Có lỗi xảy ra!');

// Warning toast
toastr.warning('Giỏ hàng trống!');

// Info toast
toastr.info('Đang xử lý...');

// Custom options
toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "timeOut": "3000"
};
```

**Thay thế TempData alert trong Cart:**

```cshtml
<!-- File: Views/Cart/Index.cshtml -->
<!-- XÓA đoạn này: -->
<script>
    var message = '@TempData["Message"]';
    if (message) {
        alert(message);
    }
</script>

<!-- THAY BẰNG: -->
<script>
    var message = '@TempData["Message"]';
    if (message) {
        toastr.success(message);
    }
</script>
```

---

#### Plugin 3: Slick Carousel (Product slider)

```html
<!-- Thêm vào _Layout.cshtml -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/slick-carousel@1.8.1/slick/slick.css">
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/slick-carousel@1.8.1/slick/slick-theme.css">
<script src="https://cdn.jsdelivr.net/npm/slick-carousel@1.8.1/slick/slick.min.js"></script>
```

**Sử dụng:**

```javascript
$(document).ready(function(){
    $('.products').slick({
        infinite: true,
        slidesToShow: 4,
        slidesToScroll: 1,
        autoplay: true,
        autoplaySpeed: 2000,
        responsive: [
            {
                breakpoint: 1024,
                settings: {
                    slidesToShow: 3
                }
            },
            {
                breakpoint: 600,
                settings: {
                    slidesToShow: 2
                }
            },
            {
                breakpoint: 480,
                settings: {
                    slidesToShow: 1
                }
            }
        ]
    });
});
```

---

## 📋 CHECKLIST KHI SỬA CSS/JQUERY

### CSS:
- [ ] Backup file gốc trước khi sửa
- [ ] Dùng `!important` khi override Bootstrap (hoặc tăng specificity)
- [ ] Test trên nhiều trình duyệt (Chrome, Firefox, Edge)
- [ ] Test responsive (mobile, tablet, desktop)
- [ ] Clear browser cache (Ctrl + F5)
- [ ] Minify CSS trước khi deploy production

### jQuery:
- [ ] Luôn wrap code trong `$(document).ready()`
- [ ] Check jQuery đã load chưa: `console.log($)`
- [ ] Test trên Console trước (F12 → Console)
- [ ] Handle errors với try-catch
- [ ] Optimize selectors (cache jQuery objects)
- [ ] Minify JS trước khi deploy

---

## 🐛 DEBUG CSS/JQUERY

### Debug CSS:

**Cách 1: Browser DevTools (F12)**

1. Right click → Inspect Element
2. Tab "Elements" → Xem HTML structure
3. Tab "Styles" → Xem CSS áp dụng
4. Checkbox bật/tắt CSS rule
5. Edit CSS real-time

**Cách 2: CSS override không work?**

Check thứ tự ưu tiên:
```
!important > inline style > ID selector > class selector > tag selector
```

Tăng specificity:
```css
/* Yếu */
.btn { color: red; }

/* Mạnh hơn */
.navbar .btn { color: red; }

/* Mạnh nhất */
.navbar .btn.btn-primary { color: red !important; }
```

---

### Debug jQuery:

**Cách 1: Console.log**

```javascript
$(document).ready(function() {
    console.log('jQuery version:', $.fn.jquery);

    var $element = $('.product');
    console.log('Found products:', $element.length);

    $element.each(function(index) {
        console.log('Product ' + index, $(this).html());
    });
});
```

**Cách 2: jQuery selector không tìm thấy element?**

```javascript
// Check element có tồn tại không
if ($('.product').length === 0) {
    console.error('Không tìm thấy .product element!');
}

// Check timing issue (element chưa load)
$(document).ready(function() {
    // Code ở đây đảm bảo DOM đã load
});

// Hoặc dùng timeout
setTimeout(function() {
    // Code ở đây chạy sau 500ms
}, 500);
```

**Cách 3: Event không fire?**

```javascript
// Cách 1: Trực tiếp (không work với dynamic elements)
$('.btn').click(function() {
    console.log('Clicked!');
});

// Cách 2: Event delegation (work với dynamic elements)
$(document).on('click', '.btn', function() {
    console.log('Clicked!');
});
```

---

## 💡 MẸO HAY

### CSS:
1. **Dùng CSS Variables** (custom properties)
```css
:root {
    --primary-color: #3498db;
    --secondary-color: #2ecc71;
    --border-radius: 8px;
}

.btn-primary {
    background-color: var(--primary-color) !important;
    border-radius: var(--border-radius) !important;
}
```

2. **Dùng CSS Grid/Flexbox** thay vì float
```css
.products {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 20px;
}
```

3. **Mobile-first approach**
```css
/* Base styles cho mobile */
.container {
    width: 100%;
}

/* Desktop styles */
@media (min-width: 768px) {
    .container {
        width: 750px;
    }
}
```

---

### jQuery:
1. **Cache jQuery selectors**
```javascript
// BAD
$('.product').show();
$('.product').addClass('active');
$('.product').css('color', 'red');

// GOOD
var $product = $('.product');
$product.show();
$product.addClass('active');
$product.css('color', 'red');
```

2. **Chain methods**
```javascript
// BAD
$('.product').show();
$('.product').addClass('active');
$('.product').css('color', 'red');

// GOOD
$('.product')
    .show()
    .addClass('active')
    .css('color', 'red');
```

3. **Dùng vanilla JS khi có thể** (faster)
```javascript
// jQuery
$('#myElement').text('Hello');

// Vanilla JS (faster)
document.getElementById('myElement').textContent = 'Hello';
```

---

## 📚 TÀI LIỆU THAM KHẢO

1. **Bootstrap 3 Docs:** https://getbootstrap.com/docs/3.4/
2. **jQuery 3 Docs:** https://api.jquery.com/
3. **CSS Tricks:** https://css-tricks.com/
4. **MDN Web Docs:** https://developer.mozilla.org/
5. **Can I Use:** https://caniuse.com/ (check browser compatibility)

---

## 🎓 BÀI TẬP THỰC HÀNH

### Bài 1: Đổi màu theme toàn website
- [ ] Đổi navbar từ đen → gradient xanh-tím
- [ ] Đổi button primary từ xanh dương → xanh lá
- [ ] Đổi font chữ toàn website

### Bài 2: Thêm animation
- [ ] Hover vào product → scale lên 1.05x
- [ ] Click button → loading spinner
- [ ] Alert message → fade out sau 3s

### Bài 3: Form validation
- [ ] Validate email format
- [ ] Validate password length >= 6
- [ ] Hiển thị error message đỏ bên dưới input

### Bài 4: AJAX cart update
- [ ] Thay đổi số lượng → auto update (không reload page)
- [ ] Xóa sản phẩm → animate fade out
- [ ] Hiển thị toast notification

---

Có thắc mắc gì cứ hỏi tôi nhé! 😊
