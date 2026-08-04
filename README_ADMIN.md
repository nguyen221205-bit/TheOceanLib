# 🛡️ TÀI LIỆU HƯỚNG DẪN & PHÂN CÔNG THỰC HIỆN PHÂN HỆ ADMIN
> **Hệ Thống Quản Lý Thư Viện - TheOceanLib**  
> *Tài liệu dành cho nhóm phát triển (Teammates) xây dựng và kiểm thử phân hệ Quản trị viên.*

---

## 🔑 I. THÔNG TIN DÙNG THỬ & ĐÃ BẢO VỆ PHÂN QUYỀN

### 1. Tài khoản Admin thử nghiệm
- **Email**: `admin@library.edu.vn`
- **Mật khẩu**: `123456` *(Đã mã hóa BCrypt trong CSDL)*
- **Quyền hạn**: `MaVaiTro == 1` (Admin)

### 2. Cơ chế phân quyền tập trung
Phân hệ Admin được bảo vệ bởi bộ lọc `OnActionExecuting` trong `AdminController.cs`:
```csharp
// Chỉ tài khoản có MaVaiTro == 1 mới truy cập được các đường dẫn /Admin/...
if (user == null) 
    filterContext.Result = RedirectToAction("DangNhap", "Home");
else if (user.MaVaiTro != 1) 
    filterContext.Result = RedirectToAction("Dashboard", "ThuThu");
```

---

## 📌 II. HƯỚNG DẪN CÁC CHỨC NĂNG CỦA ADMIN

Layout Admin đã được dựng sẵn tại `Views/Shared/_LayoutAdmin.cshtml` với đầy đủ Menu bên Sidebar:

### 1. Dashboard Admin (`/Admin/Index`)
- Xem 4 thẻ thống kê tổng quan: Tổng đầu sách, Tổng bản sách vật lý, Tổng số tài khoản, Doanh thu phạt.
- Các khối điều hướng nhanh vào từng phân hệ.

### 2. Quản Lý Sách & Kho Bản Sách (`/Admin/QuanLySach`)
- **Quản lý Đầu Sách**: 
  - Xem danh sách sách (Ảnh bìa, Tên sách, Tác giả, Thể loại, NXB, Giá bìa).
  - Thêm mới đầu sách ➡️ Có khung Tải ảnh bìa xem trước, chọn Thể loại/Tác giả/NXB động từ CSDL.
  - Sửa thông tin hoặc Xóa đầu sách (Có kiểm tra ràng buộc khóa ngoại CSDL).
- **Quản lý Bản Sách Vật Lý**:
  - Nhấn nút **Quản lý kho** ở từng đầu sách ➡️ Mở Modal danh sách cuốn sách vật lý (`#CS...`).
  - Sửa vị trí kệ (ví dụ: `Kệ A1-02`) hoặc Trạng thái (*Có sẵn*, *Đang mượn*, *Hỏng*, *Mất*).
  - **Thêm nhanh hàng loạt**: Nhập vị trí kệ + Số lượng ➡️ Tự động sinh `N` cuốn sách mới vào kho CSDL.

### 3. Quản Lý Danh Mục Hệ Thống (`/Admin/QuanLyTheLoai`, `QuanLyTacGia`, `QuanLyNXB`)
- Danh sách 3 danh mục chính: **Thể loại**, **Tác giả**, **Nhà xuất bản**.
- Thao tác Thêm / Sửa / Xóa nhanh gọn qua AJAX không nạp lại trang.

### 4. Quản Lý Tài Khoản & Phân Quyền (`/Admin/QuanLyTaiKhoan`)
- Danh sách toàn bộ tài khoản trong thư viện (Độc giả, Thủ thư, Admin).
- **Phân quyền vai trò**: Nâng quyền Độc giả ➡️ Thủ thư hoặc ngược lại.
- **Khóa / Mở khóa tài khoản**: Vô hiệu hóa tài khoản vi phạm quy định.
- **Tạo Thủ thư mới**: Thêm tài khoản thủ thư trực tiếp (mật khẩu tự động băm mã hóa BCrypt).
- **Reset Mật Khẩu**: Đặt lại mật khẩu tài khoản về `123456`.

### 5. Báo Cáo Thống Kê & Doanh Thu (`/Admin/BaoCaoThongKe`)
- Biểu đồ phân tích doanh thu phạt theo thời gian và cơ cấu sách được mượn nhiều.
- Xuất báo cáo thống kê dữ liệu.

---

## 📋 III. PHÂN CÔNG CÔNG VIỆC CHO TEAMMATES (CHECKLIST)

Nhóm phát triển hãy chia nhỏ tệp DTO và Actions theo danh sách dưới đây để hoàn thiện từng phần:

### 👤 Teammate A: Phụ trách Module 1 (Quản lý Sách & Kho)
- [ ] Tạo tệp DTO `Models/DTOs/SachAdminDTOs.cs`.
- [ ] Viết các Action `QuanLySach`, `LuuSach`, `XoaSach`, `GetDanhSachCuonSach`, `ThemCuonSach` trong `AdminController.cs`.
- [ ] Cập nhật giao diện View `Views/Admin/QuanLySach.cshtml` (Modal Thêm/Sửa sách & Modal Kho bản sách).

### 👤 Teammate B: Phụ trách Module 2 & 3 (Danh mục & Tài khoản)
- [ ] Triển khai CRUD Thể loại, Tác giả, NXB (`QuanLyTheLoai`, `QuanLyTacGia`, `QuanLyNXB`).
- [ ] Tạo DTO `Models/DTOs/TaiKhoanAdminDTOs.cs`.
- [ ] Viết Action Quản lý Tài khoản, Nâng/hạ quyền, Khóa/Mở tài khoản, Tạo thủ thư (`QuanLyTaiKhoan`).

---

## 🧪 IV. KỊCH BẢN KIỂM THỬ PHÂN HỆ ADMIN (TEST SCENARIOS)

### 🔴 Test Case 1: Kiểm thử phân quyền truy cập Admin
1. Đăng nhập tài khoản Độc giả `docgia_levan@library.edu.vn` (Pass `123456`).
2. Cố tình gõ trực tiếp URL `http://localhost:.../Admin/Index`.
3. **Kỳ vọng**: Hệ thống chặn lại và tự động đẩy về trang chủ độc giả (hoặc `/ThuThu/Dashboard`).
4. Đăng nhập tài khoản Admin `admin@library.edu.vn` (Pass `123456`).
5. **Kỳ vọng**: Cho phép truy cập `/Admin/Index`, hiển thị đầy đủ Menu Admin màu đỏ trên Header & Sidebar.

### 🟢 Test Case 2: Kiểm thử Thêm mới Đầu sách & Bản sách kho
1. Vào `/Admin/QuanLySach` ➡️ Bấm **Thêm Đầu Sách Mới**.
2. Nhập thông tin: Tên sách: *"Lập Trình C# Nâng Cao"*, chọn Ảnh bìa, chọn Thể loại, Tác giả, NXB ➡️ Nhấn **Lưu**.
3. **Kỳ vọng**: Sách mới xuất hiện trong danh sách.
4. Bấm nút **Quản lý kho** tại sách vừa tạo ➡️ Nhập Vị trí kệ: `"Kệ A5"`, Số lượng: `5` ➡️ Nhấn **Thêm kho**.
5. **Kỳ vọng**: CSDL tự động tạo 5 cuốn sách vật lý `#CS...` với vị trí `Kệ A5` và trạng thái `"Có sẵn"`.

### 🔵 Test Case 3: Kiểm thử Quản lý Tài khoản & Phân quyền
1. Vào `/Admin/QuanLyTaiKhoan`.
2. Tìm tài khoản độc giả ➡️ Nhấn **Đổi vai trò** sang **Thủ thư**.
3. **Kỳ vọng**: Vai trò đổi thành Thủ thư (`MaVaiTro = 2`), tài khoản đó đăng nhập sẽ vào được trang Thủ thư `/ThuThu/Dashboard`.
4. Nhấn **Khóa tài khoản** ➡️ **Kỳ vọng**: Độc giả đó đăng nhập lại sẽ báo tài khoản đã bị khóa.

---

## 🛠️ V. NGUYÊN TẮC CODE CHUẨN MVC & CLEAN CODE
1. Tất cả DTOs bắt buộc tạo file riêng trong thư mục `Models/DTOs/` (không khai báo inline trong Controller).
2. Khi chèn/cập nhật mật khẩu tài khoản người dùng, **bắt buộc dùng BCrypt**:  
   `string hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);`
3. Đăng ký các file `.cs` mới tạo vào `DoAn_LTWeb.csproj` trước khi biên dịch.
