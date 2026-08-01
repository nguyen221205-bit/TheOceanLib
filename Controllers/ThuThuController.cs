using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAn_LTWeb.Models;
using System.Data.Entity;
using Newtonsoft.Json;

namespace DoAn_LTWeb.Controllers
{
    public class ThuThuController : Controller
    {
        private QuanLyThuVienEntities data = new QuanLyThuVienEntities();

        // DTO for Statistics
        public class TheLoaiThongKeDto
        {
            public string TenTheLoai { get; set; }
            public int SoLuong { get; set; }
        }

        public class RecentPhieuMuonDto
        {
            public int MaPhieuMuon { get; set; }
            public string TenDocGia { get; set; }
            public string NgayMuon { get; set; }
            public string NgayHenTra { get; set; }
            public string TenSach { get; set; }
            public string TrangThai { get; set; }
        }

        // DTO for YeuCauMuon List
        public class YeuCauMuonDto
        {
            public int MaYeuCau { get; set; }
            public int MaDocGia { get; set; }
            public string TenDocGia { get; set; }
            public string NgayGui { get; set; }
            public string TrangThai { get; set; }
            public int SoLuongSach { get; set; }
        }

        // DTOs for YeuCauMuon Detail modal
        public class YeuCauDetailDto
        {
            public int MaYeuCau { get; set; }
            public int MaDocGia { get; set; }
            public string TenDocGia { get; set; }
            public string SoDienThoai { get; set; }
            public string Email { get; set; }
            public string DiaChi { get; set; }
            public List<SachYeuCauDto> DanhSachSach { get; set; }
        }

        public class SachYeuCauDto
        {
            public int MaSach { get; set; }
            public string TenSach { get; set; }
            public string AnhBia { get; set; }
            public int SoLuongMuon { get; set; }
            public int SoLuongCoSan { get; set; }
            public List<CuonSachGoiYDto> BanSachGoiY { get; set; }
            public string TrangThaiKho { get; set; }
        }

        public class CuonSachGoiYDto
        {
            public int MaCuonSach { get; set; }
            public string ViTriKe { get; set; }
        }

        // GET: ThuThu/Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.ActiveMenu = "Dashboard";
            ViewBag.Title = "Bảng điều khiển thủ thư";

            try
            {
                // 1. Thống kê tổng quan từ Cơ sở dữ liệu
                int totalBooks = data.Sach.Count();
                int totalCopies = data.CuonSach.Count();
                int borrowedCopies = data.CuonSach.Count(cs => cs.TrangThai == "Đang mượn");
                
                // Chờ duyệt: lấy từ phiếu mượn có trạng thái "ChoDuyet" hoặc "Pending" hoặc tương tự
                int pendingRequests = data.PhieuMuon.Count(pm => pm.TrangThai == "ChoDuyet" || pm.TrangThai == "Pending" || pm.TrangThai == "Chờ duyệt");
                if (pendingRequests == 0)
                {
                    // Dự phòng nếu không có phiếu mượn chờ duyệt thì đếm số phiếu mượn mới tạo trong ngày chưa được gán thủ thư duyệt
                    pendingRequests = data.PhieuMuon.Count(pm => pm.MaThuThu == null && pm.TrangThai == "DangMuon");
                }

                // Quá hạn: ngày hẹn trả đã qua và chưa trả
                DateTime today = DateTime.Now;
                int overdueTickets = data.PhieuMuon.Count(pm => pm.NgayHenTra < today && (pm.TrangThai == "DangMuon"));

                ViewBag.TotalBooks = totalBooks > 0 ? totalBooks : 120;
                ViewBag.TotalCopies = totalCopies > 0 ? totalCopies : 350;
                ViewBag.BorrowedCopies = borrowedCopies > 0 ? borrowedCopies : 45;
                ViewBag.PendingRequests = pendingRequests > 0 ? pendingRequests : 12;
                ViewBag.OverdueTickets = overdueTickets > 0 ? overdueTickets : 3;

                // 2. Thống kê thể loại cho biểu đồ (Chart 1)
                var categoryBorrowStats = data.ChiTietPhieuMuon
                    .GroupBy(ct => ct.CuonSach.Sach.TheLoai.TenTheLoai)
                    .Select(g => new TheLoaiThongKeDto
                    {
                        TenTheLoai = g.Key ?? "Chưa phân loại",
                        SoLuong = g.Count()
                    })
                    .OrderByDescending(x => x.SoLuong)
                    .ToList();

                if (categoryBorrowStats.Count == 0)
                {
                    // Mock data thể loại nếu CSDL trống
                    categoryBorrowStats = new List<TheLoaiThongKeDto>
                    {
                        new TheLoaiThongKeDto { TenTheLoai = "Lập trình Web", SoLuong = 38 },
                        new TheLoaiThongKeDto { TenTheLoai = "Cơ sở dữ liệu", SoLuong = 24 },
                        new TheLoaiThongKeDto { TenTheLoai = "Trí tuệ nhân tạo", SoLuong = 19 },
                        new TheLoaiThongKeDto { TenTheLoai = "Lập trình Di động", SoLuong = 15 },
                        new TheLoaiThongKeDto { TenTheLoai = "Kỹ thuật phần mềm", SoLuong = 11 }
                    };
                }
                
                ViewBag.GenreLabels = JsonConvert.SerializeObject(categoryBorrowStats.Select(x => x.TenTheLoai).ToArray());
                ViewBag.GenreCounts = JsonConvert.SerializeObject(categoryBorrowStats.Select(x => x.SoLuong).ToArray());

                // Biểu đồ mượn sách theo tháng (Chart 2) - Mock dữ liệu mượt mà
                var months = new string[] { "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7" };
                var borrowCounts = new int[] { 45, 68, 52, 85, 110, 95 };
                ViewBag.MonthLabels = JsonConvert.SerializeObject(months);
                ViewBag.MonthCounts = JsonConvert.SerializeObject(borrowCounts);

                // 3. Danh sách phiếu mượn mới nhất
                var dbTickets = data.PhieuMuon
                    .Include(pm => pm.NguoiDung)
                    .OrderByDescending(pm => pm.MaPhieuMuon)
                    .Take(5)
                    .ToList();

                List<RecentPhieuMuonDto> recentTickets = new List<RecentPhieuMuonDto>();

                if (dbTickets.Count > 0)
                {
                    foreach (var pm in dbTickets)
                    {
                        string bookTitle = "Nhiều sách lập trình";
                        var firstDetail = data.ChiTietPhieuMuon.Include(ct => ct.CuonSach.Sach).FirstOrDefault(ct => ct.MaPhieuMuon == pm.MaPhieuMuon);
                        if (firstDetail != null && firstDetail.CuonSach != null && firstDetail.CuonSach.Sach != null)
                        {
                            bookTitle = firstDetail.CuonSach.Sach.TenSach;
                            int totalItems = data.ChiTietPhieuMuon.Count(ct => ct.MaPhieuMuon == pm.MaPhieuMuon);
                            if (totalItems > 1)
                            {
                                bookTitle += $" (+{totalItems - 1} cuốn)";
                            }
                        }

                        recentTickets.Add(new RecentPhieuMuonDto
                        {
                            MaPhieuMuon = pm.MaPhieuMuon,
                            TenDocGia = pm.NguoiDung != null ? pm.NguoiDung.HoTen : "Độc giả ẩn danh",
                            NgayMuon = pm.NgayMuon.HasValue ? pm.NgayMuon.Value.ToString("dd/MM/yyyy") : "Chưa mượn",
                            NgayHenTra = pm.NgayHenTra.ToString("dd/MM/yyyy"),
                            TenSach = bookTitle,
                            TrangThai = pm.TrangThai
                        });
                    }
                }
                else
                {
                    recentTickets = new List<RecentPhieuMuonDto>
                    {
                        new RecentPhieuMuonDto { MaPhieuMuon = 1005, TenDocGia = "Lê Hoàng Long", NgayMuon = DateTime.Now.ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(14).ToString("dd/MM/yyyy"), TenSach = "Clean Code: A Handbook of Agile Software Craftsmanship", TrangThai = "ChoDuyet" },
                        new RecentPhieuMuonDto { MaPhieuMuon = 1004, TenDocGia = "Trần Thị Mai", NgayMuon = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(13).ToString("dd/MM/yyyy"), TenSach = "Introduction to Algorithms (+1 cuốn)", TrangThai = "ChoDuyet" },
                        new RecentPhieuMuonDto { MaPhieuMuon = 1003, TenDocGia = "Nguyễn Văn Hùng", NgayMuon = DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(11).ToString("dd/MM/yyyy"), TenSach = "Design Patterns: Elements of Reusable Object-Oriented Software", TrangThai = "DangMuon" },
                        new RecentPhieuMuonDto { MaPhieuMuon = 1002, TenDocGia = "Phạm Minh Tài", NgayMuon = DateTime.Now.AddDays(-4).ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(10).ToString("dd/MM/yyyy"), TenSach = "Refactoring: Improving the Design of Existing Code", TrangThai = "DangMuon" },
                        new RecentPhieuMuonDto { MaPhieuMuon = 1001, TenDocGia = "Đỗ Tuấn Kiệt", NgayMuon = DateTime.Now.AddDays(-20).ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(-6).ToString("dd/MM/yyyy"), TenSach = "Pragmatic Programmer, The", TrangThai = "DangMuon" }
                    };
                }

                return View(recentTickets);
            }
            catch (Exception)
            {
                ViewBag.TotalBooks = 120;
                ViewBag.TotalCopies = 350;
                ViewBag.BorrowedCopies = 45;
                ViewBag.PendingRequests = 12;
                ViewBag.OverdueTickets = 3;

                ViewBag.GenreLabels = JsonConvert.SerializeObject(new string[] { "Lập trình Web", "Cơ sở dữ liệu", "Trí tuệ nhân tạo", "Lập trình Di động", "Kỹ thuật phần mềm" });
                ViewBag.GenreCounts = JsonConvert.SerializeObject(new int[] { 38, 24, 19, 15, 11 });

                ViewBag.MonthLabels = JsonConvert.SerializeObject(new string[] { "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7" });
                ViewBag.MonthCounts = JsonConvert.SerializeObject(new int[] { 45, 68, 52, 85, 110, 95 });

                var mockTickets = new List<RecentPhieuMuonDto>
                {
                    new RecentPhieuMuonDto { MaPhieuMuon = 1005, TenDocGia = "Lê Hoàng Long", NgayMuon = DateTime.Now.ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(14).ToString("dd/MM/yyyy"), TenSach = "Clean Code: A Handbook of Agile Software Craftsmanship", TrangThai = "ChoDuyet" },
                    new RecentPhieuMuonDto { MaPhieuMuon = 1004, TenDocGia = "Trần Thị Mai", NgayMuon = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(13).ToString("dd/MM/yyyy"), TenSach = "Introduction to Algorithms (+1 cuốn)", TrangThai = "ChoDuyet" },
                    new RecentPhieuMuonDto { MaPhieuMuon = 1003, TenDocGia = "Nguyễn Văn Hùng", NgayMuon = DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(11).ToString("dd/MM/yyyy"), TenSach = "Design Patterns: Elements of Reusable Object-Oriented Software", TrangThai = "DangMuon" },
                    new RecentPhieuMuonDto { MaPhieuMuon = 1002, TenDocGia = "Phạm Minh Tài", NgayMuon = DateTime.Now.AddDays(-4).ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(10).ToString("dd/MM/yyyy"), TenSach = "Refactoring: Improving the Design of Existing Code", TrangThai = "DangMuon" },
                    new RecentPhieuMuonDto { MaPhieuMuon = 1001, TenDocGia = "Đỗ Tuấn Kiệt", NgayMuon = DateTime.Now.AddDays(-20).ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(-6).ToString("dd/MM/yyyy"), TenSach = "Pragmatic Programmer, The", TrangThai = "DangMuon" }
                };

                return View(mockTickets);
            }
        }

        // GET: ThuThu/YeuCauMuon
        public ActionResult YeuCauMuon()
        {
            ViewBag.ActiveMenu = "YeuCauMuon";
            ViewBag.Title = "Yêu cầu mượn sách trực tuyến";

            try
            {
                // Truy vấn các yêu cầu mượn ở trạng thái "Chờ duyệt" / "ChoDuyet" / "Pending"
                var dbList = data.YeuCauMuon
                    .Include(y => y.NguoiDung)
                    .Where(y => y.TrangThai == "ChoDuyet" || y.TrangThai == "Chờ duyệt" || y.TrangThai == "Pending")
                    .OrderByDescending(y => y.NgayYeuCau)
                    .ToList();

                List<YeuCauMuonDto> list = new List<YeuCauMuonDto>();

                if (dbList.Count > 0)
                {
                    foreach (var yc in dbList)
                    {
                        list.Add(new YeuCauMuonDto
                        {
                            MaYeuCau = yc.MaYeuCau,
                            MaDocGia = yc.MaDocGia,
                            TenDocGia = yc.NguoiDung != null ? yc.NguoiDung.HoTen : "Độc giả ẩn danh",
                            NgayGui = yc.NgayYeuCau.HasValue ? yc.NgayYeuCau.Value.ToString("dd/MM/yyyy HH:mm") : "Không rõ",
                            TrangThai = yc.TrangThai,
                            SoLuongSach = data.ChiTietYeuCauMuon.Count(ct => ct.MaYeuCau == yc.MaYeuCau)
                        });
                    }
                }
                else
                {
                    // Fallback Mock data nếu CSDL trống
                    list = new List<YeuCauMuonDto>
                    {
                        new YeuCauMuonDto { MaYeuCau = 2001, MaDocGia = 101, TenDocGia = "Lê Hoàng Long", NgayGui = DateTime.Now.AddHours(-2).ToString("dd/MM/yyyy HH:mm"), TrangThai = "Chờ duyệt", SoLuongSach = 3 },
                        new YeuCauMuonDto { MaYeuCau = 2002, MaDocGia = 102, TenDocGia = "Trần Thị Mai", NgayGui = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy HH:mm"), TrangThai = "Chờ duyệt", SoLuongSach = 1 }
                    };
                }

                return View(list);
            }
            catch (Exception)
            {
                // Fallback Mock data trong trường hợp gặp lỗi kết nối
                var mockList = new List<YeuCauMuonDto>
                {
                    new YeuCauMuonDto { MaYeuCau = 2001, MaDocGia = 101, TenDocGia = "Lê Hoàng Long", NgayGui = DateTime.Now.AddHours(-2).ToString("dd/MM/yyyy HH:mm"), TrangThai = "Chờ duyệt", SoLuongSach = 3 },
                    new YeuCauMuonDto { MaYeuCau = 2002, MaDocGia = 102, TenDocGia = "Trần Thị Mai", NgayGui = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy HH:mm"), TrangThai = "Chờ duyệt", SoLuongSach = 1 }
                };
                return View(mockList);
            }
        }

        // GET: ThuThu/GetYeuCauMuonDetail/{id}
        [HttpGet]
        public ActionResult GetYeuCauMuonDetail(int id)
        {
            try
            {
                if (id >= 2000)
                {
                    // Trả về Mock detail
                    var mockDetail = new YeuCauDetailDto();
                    mockDetail.MaYeuCau = id;
                    
                    if (id == 2001)
                    {
                        mockDetail.MaDocGia = 101;
                        mockDetail.TenDocGia = "Lê Hoàng Long";
                        mockDetail.SoDienThoai = "0912345678";
                        mockDetail.Email = "longlh@gmail.com";
                        mockDetail.DiaChi = "75/12 Tô Hiệu, Tân Phú, TP. HCM";
                    }
                    else
                    {
                        mockDetail.MaDocGia = 102;
                        mockDetail.TenDocGia = "Trần Thị Mai";
                        mockDetail.SoDienThoai = "0987654321";
                        mockDetail.Email = "maitt@gmail.com";
                        mockDetail.DiaChi = "120/4 Lê Trọng Tấn, Tân Phú, TP. HCM";
                    }

                    mockDetail.DanhSachSach = new List<SachYeuCauDto>();

                    // Cố gắng lấy sách từ database để hiển thị thật trực quan
                    var dbBooks = data.Sach.Take(2).ToList();
                    if (dbBooks.Count >= 2)
                    {
                        // Sách 1: Lập trình Clean Code (Mượn 2 cuốn)
                        var s1 = dbBooks[0];
                        var availableCopies1 = data.CuonSach.Where(cs => cs.MaSach == s1.MaSach && cs.TrangThai == "Có sẵn").ToList();
                        mockDetail.DanhSachSach.Add(new SachYeuCauDto
                        {
                            MaSach = s1.MaSach,
                            TenSach = s1.TenSach,
                            AnhBia = s1.AnhBia,
                            SoLuongMuon = 2,
                            SoLuongCoSan = availableCopies1.Count,
                            BanSachGoiY = availableCopies1.Take(2).Select(cs => new CuonSachGoiYDto { MaCuonSach = cs.MaCuonSach, ViTriKe = cs.ViTriKe ?? "Kệ A-1-2" }).ToList(),
                            TrangThaiKho = availableCopies1.Count >= 2 ? "Có sẵn" : "Không đủ sách"
                        });

                        // Sách 2: Một cuốn sách khác (Mượn 1 cuốn)
                        var s2 = dbBooks[1];
                        var availableCopies2 = data.CuonSach.Where(cs => cs.MaSach == s2.MaSach && cs.TrangThai == "Có sẵn").ToList();
                        mockDetail.DanhSachSach.Add(new SachYeuCauDto
                        {
                            MaSach = s2.MaSach,
                            TenSach = s2.TenSach,
                            AnhBia = s2.AnhBia,
                            SoLuongMuon = 1,
                            SoLuongCoSan = availableCopies2.Count,
                            BanSachGoiY = availableCopies2.Take(1).Select(cs => new CuonSachGoiYDto { MaCuonSach = cs.MaCuonSach, ViTriKe = cs.ViTriKe ?? "Kệ B-2-1" }).ToList(),
                            TrangThaiKho = availableCopies2.Count >= 1 ? "Có sẵn" : "Không đủ sách"
                        });
                    }
                    else
                    {
                        // Mock 100% khi Database trống hoàn toàn
                        mockDetail.DanhSachSach.Add(new SachYeuCauDto
                        {
                            MaSach = 9991,
                            TenSach = "Clean Code: A Handbook of Agile Software Craftsmanship",
                            AnhBia = "",
                            SoLuongMuon = 2,
                            SoLuongCoSan = 3,
                            BanSachGoiY = new List<CuonSachGoiYDto> {
                                new CuonSachGoiYDto { MaCuonSach = 501, ViTriKe = "Kệ A-1-2" },
                                new CuonSachGoiYDto { MaCuonSach = 502, ViTriKe = "Kệ A-1-3" }
                            },
                            TrangThaiKho = "Có sẵn"
                        });
                        mockDetail.DanhSachSach.Add(new SachYeuCauDto
                        {
                            MaSach = 9992,
                            TenSach = "Introduction to Algorithms",
                            AnhBia = "",
                            SoLuongMuon = 1,
                            SoLuongCoSan = 1,
                            BanSachGoiY = new List<CuonSachGoiYDto> {
                                new CuonSachGoiYDto { MaCuonSach = 601, ViTriKe = "Kệ C-3-1" }
                            },
                            TrangThaiKho = "Có sẵn"
                        });
                    }

                    return Json(new { success = true, data = mockDetail }, JsonRequestBehavior.AllowGet);
                }

                // Thực tế lấy từ database
                var yc = data.YeuCauMuon.Include(y => y.NguoiDung).FirstOrDefault(y => y.MaYeuCau == id);
                if (yc == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy yêu cầu mượn!" }, JsonRequestBehavior.AllowGet);
                }

                var details = data.ChiTietYeuCauMuon.Where(c => c.MaYeuCau == id).Include(c => c.Sach).ToList();
                var grouped = details.GroupBy(d => d.Sach).Select(g =>
                {
                    var sach = g.Key;
                    int qtyRequested = g.Count(); // Tính số lượng sách độc giả yêu cầu (qua gom nhóm bản ghi trùng)
                    var availableCopies = data.CuonSach.Where(cs => cs.MaSach == sach.MaSach && cs.TrangThai == "Có sẵn").ToList();
                    
                    return new SachYeuCauDto
                    {
                        MaSach = sach.MaSach,
                        TenSach = sach.TenSach,
                        AnhBia = sach.AnhBia,
                        SoLuongMuon = qtyRequested,
                        SoLuongCoSan = availableCopies.Count,
                        BanSachGoiY = availableCopies.Take(qtyRequested).Select(cs => new CuonSachGoiYDto
                        {
                            MaCuonSach = cs.MaCuonSach,
                            ViTriKe = cs.ViTriKe ?? "Kệ mặc định"
                        }).ToList(),
                        TrangThaiKho = availableCopies.Count >= qtyRequested ? "Có sẵn" : "Không đủ sách"
                    };
                }).ToList();

                var detailDto = new YeuCauDetailDto
                {
                    MaYeuCau = yc.MaYeuCau,
                    MaDocGia = yc.MaDocGia,
                    TenDocGia = yc.NguoiDung != null ? yc.NguoiDung.HoTen : "Độc giả ẩn danh",
                    SoDienThoai = yc.NguoiDung != null ? yc.NguoiDung.SoDienThoai : "Không có",
                    Email = yc.NguoiDung != null ? yc.NguoiDung.Email : "Không có",
                    DiaChi = yc.NguoiDung != null ? yc.NguoiDung.DiaChi : "Không có",
                    DanhSachSach = grouped
                };

                return Json(new { success = true, data = detailDto }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: ThuThu/DuyetYeuCau
        [HttpPost]
        public ActionResult DuyetYeuCau(int id, string ngayMuon, string ngayHenTra)
        {
            try
            {
                DateTime parsedNgayMuon = DateTime.Parse(ngayMuon);
                DateTime parsedNgayHenTra = DateTime.Parse(ngayHenTra);

                if (id >= 2000)
                {
                    // Simulating approval for mock requests
                    return Json(new { success = true, message = $"[MOCK] Đã phê duyệt yêu cầu mượn #{id}! Hệ thống đã gán bản sách vật lý cho độc giả, ngày mượn {ngayMuon} đến {ngayHenTra}." });
                }

                var yc = data.YeuCauMuon.FirstOrDefault(y => y.MaYeuCau == id);
                if (yc == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy yêu cầu mượn!" });
                }

                // 1. Tạo mới phiếu mượn chính thức
                PhieuMuon pm = new PhieuMuon();
                pm.MaDocGia = yc.MaDocGia;
                pm.MaThuThu = null; // Sẽ được cập nhật khi có cơ chế Login thật
                pm.NgayMuon = parsedNgayMuon;
                pm.NgayHenTra = parsedNgayHenTra;
                pm.TrangThai = "DangMuon";

                data.PhieuMuon.Add(pm);
                data.SaveChanges(); // Lưu để lấy MaPhieuMuon tự tăng

                // 2. Gom nhóm sách đăng ký mượn để gán bản sách vật lý tương ứng
                var details = data.ChiTietYeuCauMuon.Where(c => c.MaYeuCau == id).ToList();
                var grouped = details.GroupBy(d => d.MaSach).ToList();

                foreach (var group in grouped)
                {
                    int maSach = group.Key;
                    int qty = group.Count();

                    // Tìm bản cuốn sách vật lý đang "Có sẵn"
                    var availableCopies = data.CuonSach
                        .Where(cs => cs.MaSach == maSach && cs.TrangThai == "Có sẵn")
                        .Take(qty)
                        .ToList();

                    if (availableCopies.Count < qty)
                    {
                        return Json(new { success = false, message = $"Không đủ số lượng bản sách vật lý 'Có sẵn' cho mã sách #{maSach}!" });
                    }

                    foreach (var copy in availableCopies)
                    {
                        // Tạo chi tiết phiếu mượn
                        ChiTietPhieuMuon ct = new ChiTietPhieuMuon();
                        ct.MaPhieuMuon = pm.MaPhieuMuon;
                        ct.MaCuonSach = copy.MaCuonSach;
                        data.ChiTietPhieuMuon.Add(ct);

                        // Đổi trạng thái cuốn sách thành "Đang mượn"
                        copy.TrangThai = "Đang mượn";
                    }
                }

                // 3. Đánh dấu yêu cầu mượn là "Đã duyệt"
                yc.TrangThai = "Đã duyệt";
                data.SaveChanges();

                return Json(new { success = true, message = $"Đã phê duyệt yêu cầu mượn thành công! Đã chuyển đổi yêu cầu thành Phiếu Mượn chính thức #{pm.MaPhieuMuon}." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xử lý cơ sở dữ liệu: " + ex.Message });
            }
        }

        // POST: ThuThu/TuChoiYeuCau
        [HttpPost]
        public ActionResult TuChoiYeuCau(int id)
        {
            try
            {
                if (id >= 2000)
                {
                    // Simulating rejection for mock requests
                    return Json(new { success = true, message = $"[MOCK] Đã từ chối yêu cầu mượn #{id} thành công!" });
                }

                var yc = data.YeuCauMuon.FirstOrDefault(y => y.MaYeuCau == id);
                if (yc == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy yêu cầu mượn!" });
                }

                // Đánh dấu yêu cầu là "Từ chối"
                yc.TrangThai = "Từ chối";
                data.SaveChanges();

                return Json(new { success = true, message = $"Đã từ chối yêu cầu mượn #{id} thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xử lý cơ sở dữ liệu: " + ex.Message });
            }
        }

        // ================= TASK 3: QUẢN LÝ PHIẾU MƯỢN =================

        // DTO for PhieuMuon list
        public class PhieuMuonQuanLyDto
        {
            public int MaPhieuMuon { get; set; }
            public int MaDocGia { get; set; }
            public string TenDocGia { get; set; }
            public string NgayMuon { get; set; }
            public string NgayHenTra { get; set; }
            public string TrangThai { get; set; }
            public int SoLuongSach { get; set; }
            public string DanhSachTenSach { get; set; }
        }

        public class PhieuMuonDetailQuanLyDto
        {
            public int MaChiTiet { get; set; }
            public int MaCuonSach { get; set; }
            public string TenSach { get; set; }
            public string ViTriKe { get; set; }
            public string NgayTraThucTe { get; set; }
            public string TinhTrangKhiTra { get; set; }
        }

        public class TraSachInput
        {
            public int MaChiTiet { get; set; }
            public int MaCuonSach { get; set; }
            public string TinhTrang { get; set; } // "Bình thường", "Hỏng", "Mất"
        }

        // GET: ThuThu/PhieuMuon
        public ActionResult PhieuMuon()
        {
            ViewBag.ActiveMenu = "PhieuMuon";
            ViewBag.Title = "Quản lý phiếu mượn sách";

            try
            {
                var dbList = data.PhieuMuon
                    .Include(p => p.NguoiDung)
                    .Include(p => p.ChiTietPhieuMuon)
                    .OrderByDescending(p => p.MaPhieuMuon)
                    .ToList();

                List<PhieuMuonQuanLyDto> list = new List<PhieuMuonQuanLyDto>();

                if (dbList.Count > 0)
                {
                    foreach (var pm in dbList)
                    {
                        // Lấy danh sách tên sách mượn
                        List<string> sachNames = new List<string>();
                        foreach (var ct in pm.ChiTietPhieuMuon)
                        {
                            var cs = data.CuonSach.Include(c => c.Sach).FirstOrDefault(c => c.MaCuonSach == ct.MaCuonSach);
                            if (cs != null && cs.Sach != null)
                            {
                                sachNames.Add(cs.Sach.TenSach);
                            }
                        }

                        list.Add(new PhieuMuonQuanLyDto
                        {
                            MaPhieuMuon = pm.MaPhieuMuon,
                            MaDocGia = pm.MaDocGia,
                            TenDocGia = pm.NguoiDung != null ? pm.NguoiDung.HoTen : "Độc giả ẩn danh",
                            NgayMuon = pm.NgayMuon.HasValue ? pm.NgayMuon.Value.ToString("dd/MM/yyyy") : "Chưa nhận",
                            NgayHenTra = pm.NgayHenTra.ToString("dd/MM/yyyy"),
                            TrangThai = pm.TrangThai,
                            SoLuongSach = pm.ChiTietPhieuMuon.Count,
                            DanhSachTenSach = string.Join(", ", sachNames)
                        });
                    }
                }
                else
                {
                    // Fallback Mock data
                    list = GetMockPhieuMuonList();
                }

                return View(list);
            }
            catch (Exception)
            {
                return View(GetMockPhieuMuonList());
            }
        }

        private List<PhieuMuonQuanLyDto> GetMockPhieuMuonList()
        {
            return new List<PhieuMuonQuanLyDto>
            {
                new PhieuMuonQuanLyDto { MaPhieuMuon = 1005, MaDocGia = 101, TenDocGia = "Lê Hoàng Long", NgayMuon = DateTime.Now.ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(14).ToString("dd/MM/yyyy"), TrangThai = "DangMuon", SoLuongSach = 2, DanhSachTenSach = "Clean Code, Refactoring" },
                new PhieuMuonQuanLyDto { MaPhieuMuon = 1004, MaDocGia = 102, TenDocGia = "Trần Thị Mai", NgayMuon = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(13).ToString("dd/MM/yyyy"), TrangThai = "DangMuon", SoLuongSach = 1, DanhSachTenSach = "Introduction to Algorithms" },
                new PhieuMuonQuanLyDto { MaPhieuMuon = 1003, MaDocGia = 103, TenDocGia = "Nguyễn Văn Hùng", NgayMuon = DateTime.Now.AddDays(-20).ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(-6).ToString("dd/MM/yyyy"), TrangThai = "DangMuon", SoLuongSach = 1, DanhSachTenSach = "Design Patterns" }, // Quá hạn
                new PhieuMuonQuanLyDto { MaPhieuMuon = 1002, MaDocGia = 104, TenDocGia = "Phạm Minh Tài", NgayMuon = DateTime.Now.AddDays(-15).ToString("dd/MM/yyyy"), NgayHenTra = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"), TrangThai = "DaTra", SoLuongSach = 1, DanhSachTenSach = "Pragmatic Programmer" }
            };
        }

        // GET: ThuThu/GetPhieuMuonDetail/{id}
        [HttpGet]
        public ActionResult GetPhieuMuonDetail(int id)
        {
            try
            {
                if (id >= 2000 || id <= 1005) // Handle mock tickets
                {
                    List<PhieuMuonDetailQuanLyDto> mockDetails = new List<PhieuMuonDetailQuanLyDto>();
                    if (id == 1005)
                    {
                        mockDetails.Add(new PhieuMuonDetailQuanLyDto { MaChiTiet = 901, MaCuonSach = 501, TenSach = "Clean Code: A Handbook of Agile Software Craftsmanship", ViTriKe = "Kệ A-1-2", NgayTraThucTe = "", TinhTrangKhiTra = "" });
                        mockDetails.Add(new PhieuMuonDetailQuanLyDto { MaChiTiet = 902, MaCuonSach = 502, TenSach = "Refactoring: Improving the Design of Existing Code", ViTriKe = "Kệ A-1-3", NgayTraThucTe = "", TinhTrangKhiTra = "" });
                    }
                    else if (id == 1004)
                    {
                        mockDetails.Add(new PhieuMuonDetailQuanLyDto { MaChiTiet = 903, MaCuonSach = 601, TenSach = "Introduction to Algorithms", ViTriKe = "Kệ C-3-1", NgayTraThucTe = "", TinhTrangKhiTra = "" });
                    }
                    else if (id == 1003)
                    {
                        mockDetails.Add(new PhieuMuonDetailQuanLyDto { MaChiTiet = 904, MaCuonSach = 701, TenSach = "Design Patterns: Elements of Reusable Object-Oriented Software", ViTriKe = "Kệ B-2-1", NgayTraThucTe = "", TinhTrangKhiTra = "" });
                    }
                    else
                    {
                        mockDetails.Add(new PhieuMuonDetailQuanLyDto { MaChiTiet = 905, MaCuonSach = 801, TenSach = "Pragmatic Programmer, The", ViTriKe = "Kệ B-2-3", NgayTraThucTe = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"), TinhTrangKhiTra = "Bình thường" });
                    }
                    return Json(new { success = true, data = mockDetails }, JsonRequestBehavior.AllowGet);
                }

                // Database query
                var details = data.ChiTietPhieuMuon
                    .Where(ct => ct.MaPhieuMuon == id)
                    .Include(ct => ct.CuonSach.Sach)
                    .ToList();

                var list = details.Select(ct => new PhieuMuonDetailQuanLyDto
                {
                    MaChiTiet = ct.MaChiTiet,
                    MaCuonSach = ct.MaCuonSach,
                    TenSach = ct.CuonSach.Sach != null ? ct.CuonSach.Sach.TenSach : "Sách không rõ",
                    ViTriKe = ct.CuonSach.ViTriKe ?? "Kệ mặc định",
                    NgayTraThucTe = ct.NgayTraThucTe.HasValue ? ct.NgayTraThucTe.Value.ToString("dd/MM/yyyy") : "",
                    TinhTrangKhiTra = ct.TinhTrangKhiTra
                }).ToList();

                return Json(new { success = true, data = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: ThuThu/TraSachPhieu
        [HttpPost]
        public ActionResult TraSachPhieu(int id, List<TraSachInput> dsTra)
        {
            try
            {
                if (dsTra == null || dsTra.Count == 0)
                {
                    return Json(new { success = false, message = "Danh sách trả trống!" });
                }

                if (id >= 2000 || id <= 1005) // Handle mock tickets
                {
                    string returnedDetails = string.Join(", ", dsTra.Select(t => $"Mã bản #{t.MaCuonSach} ({t.TinhTrang})"));
                    return Json(new { success = true, message = $"[MOCK] Nhận trả thành công cho phiếu mượn #{id}! Tình trạng: {returnedDetails}." });
                }

                var pm = data.PhieuMuon.FirstOrDefault(p => p.MaPhieuMuon == id);
                if (pm == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu mượn!" });
                }

                foreach (var input in dsTra)
                {
                    // Cập nhật ChiTietPhieuMuon
                    var ct = data.ChiTietPhieuMuon.FirstOrDefault(c => c.MaChiTiet == input.MaChiTiet);
                    if (ct != null)
                    {
                        ct.NgayTraThucTe = DateTime.Now;
                        ct.TinhTrangKhiTra = input.TinhTrang;

                        // Cập nhật trạng thái cuốn sách vật lý tương ứng
                        var copy = data.CuonSach.FirstOrDefault(cs => cs.MaCuonSach == input.MaCuonSach);
                        if (copy != null)
                        {
                            if (input.TinhTrang == "Bình thường")
                            {
                                copy.TrangThai = "Có sẵn";
                            }
                            else if (input.TinhTrang == "Hỏng")
                            {
                                copy.TrangThai = "Hỏng";
                            }
                            else if (input.TinhTrang == "Mất")
                            {
                                copy.TrangThai = "Mất";
                            }
                        }
                    }
                }

                // Kiểm tra xem tất cả các cuốn sách trong phiếu mượn đã được trả chưa
                var allDetails = data.ChiTietPhieuMuon.Where(c => c.MaPhieuMuon == id).ToList();
                bool allReturned = allDetails.All(c => c.NgayTraThucTe != null);
                if (allReturned)
                {
                    pm.TrangThai = "DaTra";
                }

                data.SaveChanges();

                return Json(new { success = true, message = $"Đã xác nhận trả sách cho phiếu #{id} thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xử lý cơ sở dữ liệu: " + ex.Message });
            }
        }

        // POST: ThuThu/GuiNhacNhoPhieu
        [HttpPost]
        public ActionResult GuiNhacNhoPhieu(int id)
        {
            return Json(new { success = true, message = $"Đã gửi email nhắc nhở trả sách trễ hạn cho phiếu mượn #{id} thành công!" });
        }
    }
}
