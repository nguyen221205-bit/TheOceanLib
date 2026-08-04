# 🛡️ TÀI LIỆU HƯỚNG DẪN & ROADMAP THỰC HIỆN PHÂN HỆ ADMIN
> **Hệ Thống Quản Lý Thư Viện - TheOceanLib**  
> *Hướng dẫn từng bước dành cho 1 Lập trình viên xây dựng và kiểm thử toàn bộ phân hệ Admin.*

---

## 🔑 I. THÔNG TIN ĐĂNG NHẬP & PHÂN QUYỀN

- **Tài khoản Admin thử nghiệm**: `admin@library.edu.vn` | Mật khẩu: `123456`
- **Bộ lọc bảo vệ**: `AdminController.cs` đã được trang bị mã bộ lọc `OnActionExecuting` (chỉ cho phép tài khoản `MaVaiTro == 1` vào các đường dẫn `/Admin/...`).
- **Khung Layout**: Đã dựng sẵn tại `Views/Shared/_LayoutAdmin.cshtml` với đầy đủ menu bên Sidebar.

---

## 🚀 II. LỘ TRÌNH 4 BƯỚC THỰC HIỆN CHI TIẾT (CHO 1 DEVELOPER)

Bạn hãy thực hiện lần lượt từ **Bước 1 đến Bước 4** để hoàn thành phân hệ Admin một cách khoa học:

```
Buổi 1-2: BƯỚC 1 ➡️ Buổi 3: BƯỚC 2 ➡️ Buổi 4: BƯỚC 3 ➡️ Buổi 5: BƯỚC 4
  (Quản Lý Sách)     (Danh Mục)       (Tài Khoản)       (Báo Cáo)
```

---

### 📚 BƯỚC 1: TRIỂN KHAI QUẢN LÝ SÁCH & KHO BẢN SÁCH (`/Admin/QuanLySach`)
1. **Tạo tệp DTO**: Tạo tệp `Models/DTOs/SachAdminDTOs.cs` chứa:
   * `SachAdminDto` (thông tin đầu sách + số cuốn tồn kho).
   * `SachInputDto` (nhận dữ liệu Thêm/Sửa sách).
   * `CuonSachManageDto` (danh sách cuốn sách vật lý).
   * `ThemCuonSachInput` (nhận vị trí kệ & số lượng thêm kho).
2. **Viết Backend (`AdminController.cs`)**:
   * Viết `QuanLySach()`, `GetSachDetail(id)`, `LuuSach(input)`, `XoaSach(id)`.
   * Viết `GetDanhSachCuonSach(maSach)`, `ThemCuonSach(input)`, `CapNhatCuonSach()`.
3. **Hoàn thiện Frontend (`Views/Admin/QuanLySach.cshtml`)**:
   * Bảng danh sách đầu sách kèm ảnh bìa, giá, tác giả, thể loại.
   * Modal Thêm/Sửa sách (có ô chọn Upload ảnh xem trước).
   * Modal Quản lý kho bản sách vật lý `#CS...` (có ô nhập Thêm nhanh hàng loạt bản sách vào kệ).

---

### 🏷️ BƯỚC 2: TRIỂN KHAI QUẢN LÝ DANH MỤC MASTER DATA
Triển khai 3 màn hình danh mục với thao tác Thêm / Sửa / Xóa nhanh qua AJAX:
1. **Thể Loại (`/Admin/QuanLyTheLoai`)**: Quản lý danh mục thể loại sách.
2. **Tác Giả (`/Admin/QuanLyTacGia`)**: Quản lý hồ sơ tác giả.
3. **Nhà Xuất Bản (`/Admin/QuanLyNXB`)**: Quản lý danh sách NXB đối tác (Tên, Địa chỉ, Hotline).

---

### 👥 BƯỚC 3: TRIỂN KHAI QUẢN LÝ TÀI KHOẢN & PHÂN QUYỀN (`/Admin/QuanLyTaiKhoan`)
1. **Tạo tệp DTO**: Tạo tệp `Models/DTOs/TaiKhoanAdminDTOs.cs` (`TaiKhoanAdminDto`, `TaoThuThuInput`).
2. **Viết Backend (`AdminController.cs`)**:
   * `QuanLyTaiKhoan()`: Lấy danh sách toàn bộ người dùng.
   * `DoiVaiTro(maNguoiDung, maVaiTroMoi)`: Nâng/Hạ quyền (Độc giả ↔ Thủ thư).
   * `DoiTrangThaiTaiKhoan(maNguoiDung)`: Khóa hoặc Mở khóa tài khoản.
   * `TaoTaiKhoanThuThu(input)`: Tạo thủ thư mới (*bắt buộc dùng BCrypt băm mật khẩu: `BCrypt.Net.BCrypt.HashPassword(input.MatKhau)`*).
   * `ResetMatKhau(maNguoiDung)`: Đặt lại mật khẩu về `123456`.
3. **Hoàn thiện View (`Views/Admin/QuanLyTaiKhoan.cshtml`)**:
   * Bảng danh sách người dùng kèm nút Khóa/Mở, Nút Đổi vai trò, Modal Tạo thủ thư.

---

### 📊 BƯỚC 4: TRIỂN KHAI BÁO CÁO THỐNG KÊ & DOANH THU (`/Admin/BaoCaoThongKe`)
1. **Viết Backend (`AdminController.cs`)**:
   * Truy vấn tổng hợp số lượt mượn theo 12 tháng và doanh thu phạt thực tế từ CSDL.
2. **Hoàn thiện View (`Views/Admin/BaoCaoThongKe.cshtml`)**:
   * Vẽ biểu đồ đường / biểu đồ cột với thư viện **Chart.js** đã tích hợp sẵn.
   * Thêm nút In báo cáo / Xuất dữ liệu.

---

## 🧪 III. KỊCH BẢN KIỂM THỬ TỔNG THỂ (TEST SCENARIOS)

Sau khi làm xong từng bước, hãy tự kiểm thử theo 3 Test Case chuẩn:

1. **Test Case 1 (Phân quyền bảo vệ)**: Đăng nhập tài khoản Độc giả/Thủ thư cố tình vào `/Admin/Index` ➡️ **Kỳ vọng**: Bị đẩy về trang chủ. Đăng nhập Admin ➡️ Vào bình thường.
2. **Test Case 2 (Quản lý Sách & Kho)**: Thêm 1 đầu sách mới ➡️ Thêm 5 cuốn sách vật lý vào `Kệ A1` ➡️ **Kỳ vọng**: Số lượng kho hiển thị `5/5 cuốn có sẵn`.
3. **Test Case 3 (Phân quyền người dùng)**: Đổi vai trò 1 Độc giả sang Thủ thư ➡️ **Kỳ vọng**: Tài khoản đó đăng nhập sẽ mở được trang `/ThuThu/Dashboard`.

---

## ⚠️ IV. 3 NGUYÊN TẮC VÀNG CẦN TUÂN THỦ
1. **Dùng DTOs độc lập**: Tất cả DTOs bắt buộc tạo file riêng trong `Models/DTOs/` (không viết inline trong Controller).
2. **Bảo mật BCrypt**: Khi chèn mật khẩu mới vào CSDL, bắt buộc dùng `BCrypt.Net.BCrypt.HashPassword(...)`.
3. **Đăng ký `.csproj`**: Nhớ đăng ký file `.cs` mới tạo vào `DoAn_LTWeb.csproj` để MSBuild biên dịch thành công.
