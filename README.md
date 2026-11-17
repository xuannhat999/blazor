# SportPlus

Website bán áo bóng đá (Blazor .NET 11 + MySQL).

## Yêu cầu

- [.NET SDK 11](https://dotnet.microsoft.com/download)
- MySQL 8.0+ hoặc MariaDB 10.6+ đang chạy tại `localhost:3306`
- `git` (để clone) và `mysql` CLI

## 1. Clone repo

```bash
git clone <repo-url> blazor_app
cd blazor_app
```

## 2. Tạo database và user

Đăng nhập MySQL bằng tài khoản root rồi chạy:

```sql
CREATE DATABASE IF NOT EXISTS blazor
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

CREATE USER IF NOT EXISTS 'admin'@'localhost' IDENTIFIED BY 'admin';
GRANT ALL PRIVILEGES ON blazor.* TO 'admin'@'localhost';
FLUSH PRIVILEGES;
```

Nếu database/user đã tồn tại thì bỏ qua bước này.

## 3. Kiểm tra chuỗi kết nối

Mở `appsettings.json`, mục `ConnectionStrings:DefaultConnection`:

```json
"DefaultConnection": "Server=localhost;Port=3306;Database=blazor;User=admin;Password=admin;"
```

Sửa lại nếu MySQL của bạn dùng port, user hoặc mật khẩu khác.

Có thể ghi đè mà không sửa file:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=blazor;User=admin;Password=admin;"
```

## 4. Chạy project

```bash
dotnet restore
dotnet run
```

Mở trình duyệt: <http://localhost:5277>

Chạy profile HTTPS (tuỳ chọn):

```bash
dotnet dev-certs https --trust
dotnet run --launch-profile https
# https://localhost:7253
```

Nếu port bị chiếm, chỉ định port khác:

```bash
dotnet run --urls http://localhost:5000
```

## 5. Lần chạy đầu tiên

Ứng dụng tự tạo bảng (`EnsureCreated`) và nạp dữ liệu mẫu từ `Data/seed.json`
(131 sản phẩm, 10 người dùng, 2 đơn hàng mẫu). Các lần chạy sau sẽ bỏ qua bước
seed nếu bảng `products` đã có dữ liệu.

## Lỗi thường gặp

| Lỗi | Cách xử lý |
| --- | --- |
| `Unable to connect to any of the specified MySQL hosts` | Kiểm tra MySQL đang chạy (`systemctl status mysql`), đúng port/user/password |
| `Access denied for user 'admin'` | Chạy lại phần tạo user và `GRANT` ở bước 2 |
| `Address already in use` | Dùng `dotnet run --urls http://localhost:<port-khác>` |
| Cảnh báo `NU1902` (MySqlConnector) | Cảnh báo có sẵn, không ảnh hưởng chạy project |
