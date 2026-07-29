# 🌊 TheOceanLib - Website Quản Lý Thư Viện Lập Trình

Dự án website quản lý thư viện lập trình được xây dựng trên nền tảng **ASP.NET MVC 5** (.NET Framework 4.8) kết hợp giao diện phong cách **Ocean Theme (Đại dương)** ứng dụng thiết kế kính mờ **Glassmorphism** hiện đại.

Tài liệu này hướng dẫn cách tải, cài đặt và khởi chạy dự án trên một máy tính mới.

---

## 📋 Yêu cầu Hệ thống & Phần mềm

Trước khi tiến hành cài đặt, hãy đảm bảo máy tính đích đáp ứng các yêu cầu sau:

1.  **Hệ điều hành**: Windows 10 hoặc Windows 11 (Bắt buộc vì dự án chạy trên .NET Framework classic).
2.  **Môi trường Phát triển**: [Visual Studio 2022](https://visualstudio.microsoft.com/) (Bản Community, Professional hoặc Enterprise).
    *   *Lưu ý khi cài đặt Visual Studio*: Bạn bắt buộc phải chọn Workload **"ASP.NET and web development"** (Phát triển web và ASP.NET) để tích hợp đầy đủ công cụ chạy dự án Web Framework.
3.  **Hệ quản trị CSDL**: Microsoft SQL Server (Bản LocalDB đi kèm VS hoặc SQL Server Management Studio - SSMS) để kết nối cơ sở dữ liệu sau này.
4.  **Git**: [Git for Windows](https://git-scm.com/) (Dùng để clone dự án từ GitHub).

---

## 🚀 Các Bước Cài đặt & Khởi chạy

Thực hiện lần lượt các bước dưới đây để cài đặt dự án:

### Bước 1: Tải mã nguồn về máy mới

Mở terminal (Command Prompt, PowerShell hoặc Git Bash) trên máy tính mới và chạy lệnh clone repository:

```bash
git clone https://github.com/nguyen221205-bit/TheOceanLib.git
```

Hoặc tải tệp ZIP trực tiếp từ GitHub của dự án và giải nén ra thư mục làm việc của bạn.

---

### Bước 2: Mở dự án trong Visual Studio

1.  Mở **Visual Studio 2022**.
2.  Chọn **Open a project or solution** (Mở dự án hoặc giải pháp).
3.  Tìm đến thư mục dự án vừa tải về, chọn tệp giải pháp **`DoAn_LTWeb.sln`** và nhấn **Open**.

---

### Bước 3: Khôi phục thư viện NuGet (Restore NuGet Packages)

Các thư viện ngoài (như Bootstrap, jQuery, WebGrease, v.v.) đã bị lược bỏ khi đẩy lên Git thông qua cấu hình `.gitignore`. Bạn cần khôi phục lại chúng:

1.  **Khôi phục tự động**: Khi mở dự án, Visual Studio thường sẽ tự động tải các gói thiếu.
2.  **Khôi phục thủ công (Nếu bị lỗi thiếu thư viện)**:
    *   Nhấp chuột phải vào **Solution 'DoAn_LTWeb'** ở cửa sổ *Solution Explorer* bên phải màn hình.
    *   Chọn **Restore NuGet Packages** (Khôi phục các gói NuGet).
    *   *Cách khác:* Vào mục **Tools** > **NuGet Package Manager** > **Package Manager Console** và gõ lệnh:
        ```powershell
        Update-Package -reinstall
        ```

---

### Bước 4: Cấu hình Cơ sở dữ liệu (Nếu có)

Hiện tại dự án đang chạy với dữ liệu mô phỏng lưu trữ tạm thời qua bộ nhớ cục bộ trình duyệt (`localStorage`) để kiểm tra giao diện. Khi dự án được kết nối CSDL SQL Server ở backend:

1.  Mở tệp **`Web.config`** nằm ở thư mục gốc của dự án.
2.  Tìm đến thẻ `<connectionStrings>` và cập nhật lại chuỗi kết nối phù hợp với SQL Server cục bộ trên máy tính của bạn:
    ```xml
    <connectionStrings>
      <add name="DefaultConnection" connectionString="Data Source=TEN_MAY_TINH\SQLEXPRESS;Initial Catalog=TheOceanLib;Integrated Security=True" providerName="System.Data.SqlClient" />
    </connectionStrings>
    ```

---

### Bước 5: Build và Chạy thử dự án

1.  Nhấn phím **`F5`** hoặc bấm vào nút **IIS Express** (hình tam giác màu xanh lá cây ▷) trên thanh công cụ của Visual Studio để biên dịch và chạy dự án.
2.  Trình duyệt web mặc định của bạn sẽ tự động mở trang web tại địa chỉ:
    *   **HTTP**: `http://localhost:59039/`
    *   **HTTPS**: `https://localhost:44375/`
3.  Nếu giao diện cũ vẫn hiển thị do cache của trình duyệt, hãy nhấn tổ hợp phím **`Ctrl + F5`** (Windows) để tải lại toàn bộ tài nguyên CSS/JS mới nhất.

---

## 🛠️ Cấu trúc các Thư mục Chính trong dự án

*   `App_Start/`: Chứa cấu hình định tuyến (RouteConfig.cs) và đăng ký thư viện (BundleConfig.cs).
*   `Controllers/`: Các bộ điều khiển của mô hình MVC xử lý logic yêu cầu (ví dụ: HomeController.cs).
*   `Models/`: Chứa các lớp định nghĩa thực thể dữ liệu (Sách, Độc giả, Phiếu mượn...).
*   `Views/Shared/_Layout.cshtml`: Tệp giao diện khung chung (Header, Footer, Sidebar Offcanvas...).
*   `Views/Home/Index.cshtml`: Giao diện chính của Trang chủ (Carousel, Danh mục, Sách mới...).
*   `Content/`: Chứa các tệp phong cách CSS (`Site.css`) và thư mục ảnh mẫu (`images/`).
*   `Scripts/`: Chứa mã nguồn JavaScript của hệ thống (Bootstrap, jQuery và code tương tác giỏ mượn).
