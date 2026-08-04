# 📖 TÀI LIỆU HƯỚNG DẪN SỬ DỤNG & KỊCH BẢN KIỂM THỬ PHÂN HỆ THỦ THƯ
> **Hệ Thống Quản Lý Thư Viện - TheOceanLib**  
> *Dành cho nhóm phát triển và Tester kiểm thử toàn bộ quy trình nghiệp vụ Thủ thư.*

---

## 🔑 I. THÔNG TIN TÀI KHOẢN DÙNG THỬ (ĐÃ RESET MẬT KHẨU)

Tất cả các tài khoản thử nghiệm đã được mã hóa BCrypt bảo mật và reset về mật khẩu mặc định: **`123456`**

| Vai Trò | Email Đăng Nhập | Mật Khẩu | Quyền Hạn |
| :--- | :--- | :--- | :--- |
| **Thủ Thư (Librarian)** | `thuthu@library.edu.vn` | `123456` | Quyền duyệt mượn, nhận trả sách, lập phiếu phạt |
| **Quản Trị Viên (Admin)** | `admin@library.edu.vn` | `123456` | Quyền tối cao quản lý CSDL, sách, tài khoản & báo cáo |
| **Độc Giả (Reader)** | `docgia_levan@library.edu.vn` | `123456` | Tìm sách, tạo giỏ mượn & gửi yêu cầu mượn online |

---

## 📋 II. HƯỚNG DẪN QUY TRÌNH NGHIỆP VỤ THỦ THƯ

Phân hệ Thủ thư được thiết kế theo đúng chuẩn kiến trúc **MVC & Clean Code**, làm việc 100% với dữ liệu thực tế trong CSDL SQL Server `QuanLyThuVien`.

### 1. Bảng Điều Khiển (`/ThuThu/Dashboard`)
- **Chỉ số thực tế**: Thống kê Tổng số sách, Bản sách vật lý, Bản đang cho mượn, Yêu cầu chờ duyệt và Phiếu quá hạn.
- **Biểu đồ mượn sách**: 
  - Biểu đồ tròn/cột phân tích số lượt mượn theo từng Thể loại.
  - Biểu đồ đường mượn sách 6 tháng gần nhất từ CSDL.
- **Danh sách phiếu mượn mới nhất**: Hiển thị nhanh 5 phiếu mượn gần nhất.

### 2. Yêu Cầu Mượn Trực Tuyến (`/ThuThu/YeuCauMuon`)
- **Nguyên lý 2 bước mượn sách**: Độc giả đăng ký mượn trên web ➡️ Dữ liệu được đẩy vào bảng `YeuCauMuon` (Trạng thái: `"Chờ duyệt"`).
- **Phê duyệt yêu cầu**:
  1. Thủ thư mở màn hình Yêu cầu mượn ➡️ Bấm **Chi tiết / Phê duyệt**.
  2. Chọn bản sách vật lý khả dụng trong kho (`#CS...`) gán cho từng cuốn sách độc giả đăng ký.
  3. Nhấn **Xác nhận duyệt** ➡️ Hệ thống chuyển yêu cầu thành `"Đã duyệt"`, đồng thời tạo `PhieuMuon` chính thức và đổi trạng thái bản sách thành `"Đang mượn"`.
- **Từ chối yêu cầu**: Nếu sách hết bản có sẵn, bấm **Từ chối** và nhập lý do ➡️ Chuyển trạng thái yêu cầu sang `"Từ chối"`.

### 3. Quản Lý Phiếu Mượn (`/ThuThu/PhieuMuon`)
- Theo dõi tất cả phiếu mượn chính thức trong thư viện.
- Cung cấp các Tab bộ lọc: *Tất cả*, *Đang mượn*, *Quá hạn*, *Đã trả*.
- Xem chi tiết ngày mượn, ngày hẹn trả, danh sách các sách đang mượn và thông tin độc giả.

### 4. Quản Lý Trả Sách & Tính Phạt (`/ThuThu/TraSach`)
- Chọn phiếu mượn của độc giả mang sách tới trả tại quầy.
- **Quy tắc tính tiền phạt tự động**:
  - **Trễ hạn**: `5.000 VNĐ / ngày trễ`.
  - **Bị hỏng**: `50% giá trị bìa sách`.
  - **Bị mất**: `100% giá trị bìa sách + 20.000 VNĐ phí xử lý`.
- **Xác nhận trả**:
  - Nếu trả bình thường ➡️ Chuyển bản sách thành `"Có sẵn"`, phiếu mượn thành `"Đã trả"`.
  - Nếu có phạt ➡️ Tự động chèn bản ghi vào bảng `PhieuPhat` (trạng thái `"ChuaThanhToan"`) và tự động chuyển hướng đến trang Quản lý Phiếu phạt.

### 5. Quản Lý Phiếu Phạt & Thanh Toán (`/ThuThu/PhieuPhat`)
- **Bộ lọc 3 tab**: *Tất cả*, *Chưa thanh toán*, *Đã thanh toán* + Thanh tìm kiếm độc giả/mã phiếu.
- **Nút Chi Tiết**: Xem lý do phạt, tên sách vi phạm và tình trạng khi trả.
- **Nút Thanh Toán**: Mở Modal chọn phương thức thanh toán:
  - **Tiền mặt**: Thu tiền trực tiếp.
  - **Chuyển khoản VietQR**: Tự động sinh mã QR ngân hàng VietQR kèm cú pháp nội dung chuyển khoản.
- Sau khi xác nhận thanh toán ➡️ Trạng thái đổi sang màu xanh lá `"Đã thanh toán"` và nút thanh toán chuyển sang **màu xám khóa**.

---

## 🧪 III. KỊCH BẢN KIỂM THỬ CHI TIẾT (TEST SCENARIOS FOR TEAMMATES)

Nhóm Tester / Teammates hãy thực hiện theo đúng 8 kịch bản kiểm thử dưới đây và đối chiếu với **Kết quả kỳ vọng (Expected Output)**:

### 🔴 Scenario 1: Kiểm thử Đăng nhập & Khóa quyền truy cập URL
* **Mục tiêu**: Đảm bảo phân quyền bảo vệ URL phân hệ Thủ thư.
* **Các bước thực hiện**:
  1. Chưa đăng nhập ➡️ Truy cập trực tiếp đường dẫn `http://localhost:.../ThuThu/Dashboard`.
  2. **Kỳ vọng 1**: Trình duyệt tự động chuyển hướng về trang `/Home/DangNhap`.
  3. Nhập Email: `thuthu@library.edu.vn`, Mật khẩu: `123456` ➡️ Nhấn **Đăng nhập**.
  4. **Kỳ vọng 2**: Đăng nhập thành công, hệ thống thông báo Toast và chuyển vào trang Dashboard Thủ thư.

---

### 🟠 Scenario 2: Kiểm thử Luồng Mượn Sách 2 Bước (Độc giả Đăng ký ➡️ Thủ thư Duyệt)
* **Mục tiêu**: Xác minh dữ liệu không bị nhảy cóc vào phiếu mượn khi chưa được Thủ thư duyệt.
* **Các bước thực hiện**:
  1. Đăng nhập tài khoản Độc giả `docgia_levan@library.edu.vn` (Pass: `123456`).
  2. Chọn 1 cuốn sách bất kỳ ➡️ Bấm **Thêm vào giỏ mượn** ➡️ Nhấn **Xác nhận mượn**.
  3. **Kỳ vọng 1**: Thông báo đăng ký thành công, dữ liệu được lưu vào bảng `YeuCauMuon` với trạng thái **"Chờ duyệt"**.
  4. Đăng xuất ➡️ Đăng nhập lại tài khoản Thủ thư `thuthu@library.edu.vn`.
  5. Truy cập menu **Yêu cầu mượn** (`/ThuThu/YeuCauMuon`).
  6. **Kỳ vọng 2**: Thấy yêu cầu mượn mới của Độc giả Lê Văn Minh ở trạng thái "Chờ duyệt".
  7. Bấm nút **Phê duyệt** ➡️ Chọn bản sách vật lý khả dụng ➡️ Nhấn **Xác nhận duyệt**.
  8. **Kỳ vọng 3**: Yêu cầu chuyển sang "Đã duyệt", tự động sinh Phiếu mượn mới trong trang **Phiếu mượn** (`/ThuThu/PhieuMuon`).

---

### 🟡 Scenario 3: Kiểm thử Từ chối Yêu cầu mượn
* **Mục tiêu**: Kiểm tra tính năng từ chối khi sách không sẵn sàng.
* **Các bước thực hiện**:
  1. Tại màn hình `/ThuThu/YeuCauMuon`, chọn một yêu cầu đang "Chờ duyệt".
  2. Nhấn nút **Từ chối**.
  3. Nhập lý do từ chối: *"Sách tạm thời đang bảo trì/khôi phục"*.
  4. Nhấn **Xác nhận từ chối**.
  5. **Kỳ vọng**: Yêu cầu chuyển sang trạng thái "Từ chối", không sinh phiếu mượn và số lượng sách trong kho giữ nguyên.

---

### 🟢 Scenario 4: Kiểm thử Trả Sách Bình Thường (Không Vi Phạt)
* **Mục tiêu**: Kiểm tra quy trình nhận trả sách đúng hạn và không hỏng hóc.
* **Các bước thực hiện**:
  1. Vào menu **Trả sách** (`/ThuThu/TraSach`).
  2. Chọn một phiếu mượn đang trong hạn (Trạng thái: "Đang mượn").
  3. Giữ nguyên tình trạng cuốn sách là **"Bình thường"**.
  4. Nhấn nút **Xác Nhận Trả Sách**.
  5. **Kỳ vọng**: Thông báo trả sách thành công, cuốn sách vật lý đổi trạng thái về "Có sẵn", phiếu mượn đổi thành "Đã trả", không sinh phiếu phạt.

---

### 🔵 Scenario 5: Kiểm thử Trả Sách Vi Phạm & Lập Phiếu Phạt
* **Mục tiêu**: Kiểm tra tự động tính tiền phạt làm Hỏng/Mất sách hoặc Trễ hạn.
* **Các bước thực hiện**:
  1. Vào menu **Trả sách** (`/ThuThu/TraSach`).
  2. Chọn một phiếu mượn bất kỳ.
  3. Tại ô chọn Tình trạng cuốn sách ➡️ Chọn **"Bị hỏng"** (hoặc **"Bị mất"**).
  4. **Kỳ vọng 1**: Khung Lập Phiếu Phạt xuất hiện, tự động tính tiền phạt (50% giá bìa với sách hỏng, hoặc 100% + 20k với sách mất).
  5. Chọn trạng thái thanh toán là *"Chưa thanh toán"*.
  6. Nhấn nút **Xác Nhận Trả & Lập Phiếu Phạt**.
  7. **Kỳ vọng 2**: Trả sách thành công, hệ thống tự động lưu phiếu phạt mới vào CSDL và tự động chuyển hướng sang trang **Phiếu phạt** (`/ThuThu/PhieuPhat`).

---

### 🟣 Scenario 6: Kiểm thử Quản Lý Phiếu Phạt & Thu Tiền (Thanh Toán)
* **Mục tiêu**: Kiểm tra xử lý thu tiền phạt và khóa nút thanh toán.
* **Các bước thực hiện**:
  1. Truy cập menu **Phiếu phạt** (`/ThuThu/PhieuPhat`).
  2. Chọn Tab bộ lọc **"Chưa thanh toán"**.
  3. **Kỳ vọng 1**: Danh sách hiển thị các phiếu phạt chưa thu tiền với Badge màu vàng nhạt `Chưa thanh toán` và nút bấm màu xanh lam `Thanh toán`.
  4. Bấm nút **Chi tiết** trên một phiếu ➡️ **Kỳ vọng 2**: Modal mở ra hiển thị đầy đủ thông tin độc giả, tên sách vi phạm và lý do phạt.
  5. Bấm nút **Thanh toán** ➡️ Chọn phương thức **"Tiền mặt"** (hoặc **"Mã QR Chuyển Khoản"**).
  6. Nhấn **Xác Nhận Đã Thu Tiền**.
  7. **Kỳ vọng 3**: Thông báo thanh toán thành công, CSDL cập nhật `TrangThaiThanhToan = 'DaThanhToan'`, Badge chuyển sang xanh lá `Đã thanh toán`, và nút Thanh toán đổi sang **màu xám khóa** không thể bấm lại.

---

### ⚪ Scenario 7: Kiểm thử Bộ Lọc Tab & Ô Tìm Kiếm
* **Mục tiêu**: Đảm bảo bộ lọc dữ liệu nhanh không bị giật lag.
* **Các bước thực hiện**:
  1. Tại trang Phiếu mượn hoặc Phiếu phạt, bấm lần lượt qua các Tab (*Tất cả*, *Chưa thanh toán*, *Đã thanh toán*).
  2. **Kỳ vọng 1**: Bảng dữ liệu tự động lọc danh sách tương ứng mà không nạp lại trang.
  3. Nhập từ khóa tên độc giả (ví dụ: `Huy` hoặc `Minh`) vào ô tìm kiếm.
  4. **Kỳ vọng 2**: Bảng tự động hiển thị các dòng trùng khớp với từ khóa tìm kiếm.

---

## 🛠️ IV. KẾT LUẬN & LIÊN HỆ BẢO TRÌ
Tất cả các tính năng của phân hệ Thủ thư đã được kiểm tra biên dịch thành công 100% với **0 Error, 0 Warning**. Nếu gặp bất kỳ vấn đề phát sinh nào trong quá trình test, hãy liên hệ ngay với nhóm phát triển!
