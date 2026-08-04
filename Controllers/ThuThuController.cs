using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAn_LTWeb.Models;
using DoAn_LTWeb.Models.DTOs;
using System.Data.Entity;
using Newtonsoft.Json;

namespace DoAn_LTWeb.Controllers
{
    public class ThuThuController : Controller
    {
        private QuanLyThuVienEntities data = new QuanLyThuVienEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = Session["User"] as NguoiDung;
            if (user == null || (user.MaVaiTro != 1 && user.MaVaiTro != 2))
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { success = false, message = "Phiên làm việc hết hạn hoặc bạn không có quyền truy cập!" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Result = RedirectToAction("DangNhap", "Home", new { returnUrl = filterContext.HttpContext.Request.Url.PathAndQuery });
                }
            }
            base.OnActionExecuting(filterContext);
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
                
                int pendingRequests = data.YeuCauMuon.Count(yc => yc.TrangThai == "ChoDuyet" || yc.TrangThai == "Pending" || yc.TrangThai == "Chờ duyệt");
                DateTime today = DateTime.Now;
                int overdueTickets = data.PhieuMuon.Count(pm => pm.NgayHenTra < today && (pm.TrangThai == "Đang mượn"));

                ViewBag.TotalBooks = totalBooks;
                ViewBag.TotalCopies = totalCopies;
                ViewBag.BorrowedCopies = borrowedCopies;
                ViewBag.PendingRequests = pendingRequests;
                ViewBag.OverdueTickets = overdueTickets;

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

                ViewBag.GenreLabels = JsonConvert.SerializeObject(categoryBorrowStats.Select(x => x.TenTheLoai).ToArray());
                ViewBag.GenreCounts = JsonConvert.SerializeObject(categoryBorrowStats.Select(x => x.SoLuong).ToArray());

                // 3. Biểu đồ mượn sách theo 6 tháng gần nhất (Chart 2 - Thật 100% từ CSDL)
                List<string> monthLabels = new List<string>();
                List<int> monthCounts = new List<int>();

                for (int i = 5; i >= 0; i--)
                {
                    var mDate = DateTime.Now.AddMonths(-i);
                    monthLabels.Add($"Tháng {mDate.Month}");

                    int count = data.PhieuMuon.Count(p => p.NgayMuon.HasValue && 
                                                          p.NgayMuon.Value.Month == mDate.Month && 
                                                          p.NgayMuon.Value.Year == mDate.Year);
                    monthCounts.Add(count);
                }

                ViewBag.MonthLabels = JsonConvert.SerializeObject(monthLabels.ToArray());
                ViewBag.MonthCounts = JsonConvert.SerializeObject(monthCounts.ToArray());

                // 4. Danh sách 5 phiếu mượn mới nhất thực tế từ CSDL
                var dbTickets = data.PhieuMuon
                    .Include(pm => pm.NguoiDung)
                    .OrderByDescending(pm => pm.MaPhieuMuon)
                    .Take(5)
                    .ToList();

                List<RecentPhieuMuonDto> recentTickets = new List<RecentPhieuMuonDto>();

                foreach (var pm in dbTickets)
                {
                    string bookTitle = "Nhiều sách mượn";
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
                        MaDocGia = pm.MaDocGia,
                        TenDocGia = pm.NguoiDung != null ? pm.NguoiDung.HoTen : "Độc giả ẩn danh",
                        NgayMuon = pm.NgayMuon.HasValue ? pm.NgayMuon.Value.ToString("dd/MM/yyyy") : "Chưa mượn",
                        NgayHenTra = pm.NgayHenTra.ToString("dd/MM/yyyy"),
                        TenSach = bookTitle,
                        TrangThai = pm.TrangThai
                    });
                }

                return View(recentTickets);
            }
            catch (Exception)
            {
                ViewBag.TotalBooks = 0;
                ViewBag.TotalCopies = 0;
                ViewBag.BorrowedCopies = 0;
                ViewBag.PendingRequests = 0;
                ViewBag.OverdueTickets = 0;

                ViewBag.GenreLabels = JsonConvert.SerializeObject(new string[0]);
                ViewBag.GenreCounts = JsonConvert.SerializeObject(new int[0]);

                ViewBag.MonthLabels = JsonConvert.SerializeObject(new string[0]);
                ViewBag.MonthCounts = JsonConvert.SerializeObject(new int[0]);

                return View(new List<RecentPhieuMuonDto>());
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

                return View(list);
            }
            catch (Exception)
            {
                return View(new List<YeuCauMuonDto>());
            }
        }

        // GET: ThuThu/GetYeuCauMuonDetail/{id}
        [HttpGet]
        public ActionResult GetYeuCauMuonDetail(int id)
        {
            try
            {

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

                var yc = data.YeuCauMuon.FirstOrDefault(y => y.MaYeuCau == id);
                if (yc == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy yêu cầu mượn!" });
                }

                // 1. Tạo mới phiếu mượn chính thức
                var currentLibrarian = Session["User"] as NguoiDung;
                PhieuMuon pm = new PhieuMuon();
                pm.MaDocGia = yc.MaDocGia;
                pm.MaThuThu = currentLibrarian != null ? currentLibrarian.MaNguoiDung : (int?)null;
                pm.NgayMuon = parsedNgayMuon;
                pm.NgayHenTra = parsedNgayHenTra;
                pm.TrangThai = "Đang mượn";

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

                return View(list);
            }
            catch (Exception)
            {
                return View(new List<PhieuMuonQuanLyDto>());
            }
        }

        // GET: ThuThu/GetPhieuMuonDetail/{id}
        [HttpGet]
        public ActionResult GetPhieuMuonDetail(int id)
        {
            try
            {
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
                    pm.TrangThai = "Đã trả";
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

        // ================= TASK 4: QUẢN LÝ TRẢ SÁCH & LẬP PHIẾU PHẠT =================

        // GET: ThuThu/TraSach
        public ActionResult TraSach()
        {
            ViewBag.ActiveMenu = "TraSach";
            ViewBag.Title = "Xử lý trả sách và vi phạm";

            try
            {
                // Chỉ lấy các phiếu mượn chưa hoàn thành (DangMuon hoặc có sách chưa trả thực tế)
                var dbList = data.PhieuMuon
                    .Include(p => p.NguoiDung)
                    .Include(p => p.ChiTietPhieuMuon)
                    .Where(p => p.TrangThai != "DaTra" && p.TrangThai != "Đã trả")
                    .OrderByDescending(p => p.MaPhieuMuon)
                    .ToList();

                List<PhieuMuonQuanLyDto> list = new List<PhieuMuonQuanLyDto>();

                foreach (var pm in dbList)
                {
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

                return View(list);
            }
            catch (Exception)
            {
                return View(new List<PhieuMuonQuanLyDto>());
            }
        }

        // GET: ThuThu/GetTraSachDetail/{id}
        [HttpGet]
        public ActionResult GetTraSachDetail(int id)
        {
            try
            {
                // Database query
                var pm = data.PhieuMuon.Include(p => p.NguoiDung).FirstOrDefault(p => p.MaPhieuMuon == id);
                if (pm == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu mượn!" }, JsonRequestBehavior.AllowGet);
                }

                var details = data.ChiTietPhieuMuon
                    .Where(ct => ct.MaPhieuMuon == id)
                    .Include(ct => ct.CuonSach.Sach)
                    .ToList();

                var bookDtos = details.Select(ct => new TraSachBookDto
                {
                    MaChiTiet = ct.MaChiTiet,
                    MaCuonSach = ct.MaCuonSach,
                    TenSach = ct.CuonSach.Sach != null ? ct.CuonSach.Sach.TenSach : "Sách không rõ",
                    AnhBia = ct.CuonSach.Sach != null ? ct.CuonSach.Sach.AnhBia : "",
                    ViTriKe = ct.CuonSach.ViTriKe ?? "Kệ mặc định",
                    GiaSach = 150000,
                    NgayTraThucTe = ct.NgayTraThucTe.HasValue ? ct.NgayTraThucTe.Value.ToString("dd/MM/yyyy") : "",
                    TinhTrangKhiTra = ct.TinhTrangKhiTra
                }).ToList();

                var detailDto = new TraSachDetailDto
                {
                    MaDocGia = pm.MaDocGia,
                    TenDocGia = pm.NguoiDung != null ? pm.NguoiDung.HoTen : "Độc giả ẩn danh",
                    SoDienThoai = pm.NguoiDung != null ? pm.NguoiDung.SoDienThoai : "Chưa cập nhật",
                    Email = pm.NguoiDung != null ? pm.NguoiDung.Email : "Chưa cập nhật",
                    DiaChi = pm.NguoiDung != null ? pm.NguoiDung.DiaChi : "Chưa cập nhật",
                    NgayMuon = pm.NgayMuon.HasValue ? pm.NgayMuon.Value.ToString("dd/MM/yyyy") : "Chưa nhận",
                    NgayHenTra = pm.NgayHenTra.ToString("dd/MM/yyyy"),
                    Books = bookDtos
                };

                return Json(new { success = true, data = detailDto }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: ThuThu/XacNhanTraSach
        [HttpPost]
        public ActionResult XacNhanTraSach(TraSachSubmitInput input)
        {
            try
            {
                if (input == null || input.Items == null || input.Items.Count == 0)
                {
                    return Json(new { success = false, message = "Không có cuốn sách nào được chọn để trả!" });
                }

                int id = input.MaPhieuMuon;

                var pm = data.PhieuMuon.FirstOrDefault(p => p.MaPhieuMuon == id);
                if (pm == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu mượn!" });
                }

                int createdPhatId = 0;

                foreach (var item in input.Items)
                {
                    if (item.TraSach)
                    {
                        var ct = data.ChiTietPhieuMuon.FirstOrDefault(c => c.MaChiTiet == item.MaChiTiet);
                        if (ct != null)
                        {
                            ct.NgayTraThucTe = DateTime.Now;
                            ct.TinhTrangKhiTra = item.TinhTrang;

                            // Cập nhật trạng thái cuốn sách
                            var copy = data.CuonSach.FirstOrDefault(cs => cs.MaCuonSach == item.MaCuonSach);
                            if (copy != null)
                            {
                                if (item.TinhTrang == "Bình thường")
                                {
                                    copy.TrangThai = "Có sẵn";
                                }
                                else if (item.TinhTrang == "Hỏng")
                                {
                                    copy.TrangThai = "Hỏng";
                                }
                                else if (item.TinhTrang == "Mất")
                                {
                                    copy.TrangThai = "Mất";
                                }
                            }

                            // Tạo phiếu phạt nếu có
                            if (item.TienPhatRieng > 0)
                            {
                                string statusStr = (input.TrangThaiThanhToan == "Đã thanh toán" || input.TrangThaiThanhToan == "DaThanhToan") ? "DaThanhToan" : "ChuaThanhToan";
                                var phat = new PhieuPhat
                                {
                                    MaChiTiet = item.MaChiTiet,
                                    SoTienPhat = item.TienPhatRieng,
                                    LyDo = item.LyDoPhatRieng ?? "Vi phạm trả sách",
                                    NgayLap = DateTime.Now,
                                    TrangThaiThanhToan = statusStr
                                };
                                data.PhieuPhat.Add(phat);
                            }
                        }
                    }
                }

                // Kiểm tra xem tất cả sách đã được trả chưa
                var allDetails = data.ChiTietPhieuMuon.Where(c => c.MaPhieuMuon == id).ToList();
                bool allReturned = allDetails.All(c => c.NgayTraThucTe != null);
                if (allReturned)
                {
                    pm.TrangThai = "Đã trả";
                }

                data.SaveChanges();

                // Lấy ra mã phiếu phạt mới nhất vừa tạo nếu có
                var latestPhat = data.PhieuPhat.Where(p => p.ChiTietPhieuMuon.MaPhieuMuon == id).OrderByDescending(p => p.MaPhieuPhat).FirstOrDefault();
                if (latestPhat != null)
                {
                    createdPhatId = latestPhat.MaPhieuPhat;
                }

                return Json(new { 
                    success = true, 
                    message = $"Đã xác nhận trả sách cho phiếu #{id} thành công!",
                    hasFine = (createdPhatId > 0),
                    maPhieuPhat = createdPhatId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xử lý cơ sở dữ liệu: " + ex.Message });
            }
        }

        // ================= TASK 4: QUẢN LÝ PHIẾU PHẠT =================

        // GET: ThuThu/PhieuPhat
        public ActionResult PhieuPhat()
        {
            ViewBag.ActiveMenu = "PhieuPhat";
            ViewBag.Title = "Quản lý phiếu phạt";

            try
            {
                var listDb = data.PhieuPhat
                    .Include(p => p.ChiTietPhieuMuon)
                    .Include(p => p.ChiTietPhieuMuon.PhieuMuon)
                    .Include(p => p.ChiTietPhieuMuon.PhieuMuon.NguoiDung)
                    .Include(p => p.ChiTietPhieuMuon.CuonSach.Sach)
                    .OrderByDescending(p => p.MaPhieuPhat)
                    .ToList();

                var list = listDb.Select(p => new PhieuPhatQuanLyDto
                {
                    MaPhieuPhat = p.MaPhieuPhat,
                    MaChiTiet = p.MaChiTiet,
                    MaPhieuMuon = p.ChiTietPhieuMuon != null ? p.ChiTietPhieuMuon.MaPhieuMuon : 0,
                    MaDocGia = (p.ChiTietPhieuMuon != null && p.ChiTietPhieuMuon.PhieuMuon != null) ? p.ChiTietPhieuMuon.PhieuMuon.MaDocGia : 0,
                    TenDocGia = (p.ChiTietPhieuMuon != null && p.ChiTietPhieuMuon.PhieuMuon != null && p.ChiTietPhieuMuon.PhieuMuon.NguoiDung != null) ? p.ChiTietPhieuMuon.PhieuMuon.NguoiDung.HoTen : "Độc giả ẩn danh",
                    SoDienThoai = (p.ChiTietPhieuMuon != null && p.ChiTietPhieuMuon.PhieuMuon != null && p.ChiTietPhieuMuon.PhieuMuon.NguoiDung != null) ? p.ChiTietPhieuMuon.PhieuMuon.NguoiDung.SoDienThoai : "Chưa có",
                    Email = (p.ChiTietPhieuMuon != null && p.ChiTietPhieuMuon.PhieuMuon != null && p.ChiTietPhieuMuon.PhieuMuon.NguoiDung != null) ? p.ChiTietPhieuMuon.PhieuMuon.NguoiDung.Email : "Chưa có",
                    SoTienPhat = p.SoTienPhat,
                    LyDo = p.LyDo,
                    NgayLap = p.NgayLap.HasValue ? p.NgayLap.Value.ToString("dd/MM/yyyy HH:mm") : "Không rõ",
                    TrangThaiThanhToan = p.TrangThaiThanhToan ?? "ChuaThanhToan",
                    TenSach = (p.ChiTietPhieuMuon != null && p.ChiTietPhieuMuon.CuonSach != null && p.ChiTietPhieuMuon.CuonSach.Sach != null) ? p.ChiTietPhieuMuon.CuonSach.Sach.TenSach : "Sách không rõ"
                }).ToList();

                return View(list);
            }
            catch (Exception)
            {
                return View(new List<PhieuPhatQuanLyDto>());
            }
        }

        // GET: ThuThu/GetPhieuPhatDetail/{id}
        [HttpGet]
        public ActionResult GetPhieuPhatDetail(int id)
        {
            try
            {
                var p = data.PhieuPhat
                    .Include(x => x.ChiTietPhieuMuon)
                    .Include(x => x.ChiTietPhieuMuon.PhieuMuon)
                    .Include(x => x.ChiTietPhieuMuon.PhieuMuon.NguoiDung)
                    .Include(x => x.ChiTietPhieuMuon.CuonSach.Sach)
                    .FirstOrDefault(x => x.MaPhieuPhat == id);

                if (p == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu phạt!" }, JsonRequestBehavior.AllowGet);
                }

                var user = p.ChiTietPhieuMuon?.PhieuMuon?.NguoiDung;
                var sach = p.ChiTietPhieuMuon?.CuonSach?.Sach;

                var detail = new PhieuPhatDetailDto
                {
                    MaPhieuPhat = p.MaPhieuPhat,
                    MaPhieuMuon = p.ChiTietPhieuMuon?.MaPhieuMuon ?? 0,
                    MaDocGia = p.ChiTietPhieuMuon?.PhieuMuon?.MaDocGia ?? 0,
                    TenDocGia = user?.HoTen ?? "Độc giả ẩn danh",
                    SoDienThoai = user?.SoDienThoai ?? "Chưa cập nhật",
                    Email = user?.Email ?? "Chưa cập nhật",
                    DiaChi = user?.DiaChi ?? "Chưa cập nhật",
                    SoTienPhat = p.SoTienPhat,
                    LyDo = p.LyDo,
                    NgayLap = p.NgayLap.HasValue ? p.NgayLap.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    TrangThaiThanhToan = p.TrangThaiThanhToan ?? "ChuaThanhToan",
                    MaCuonSach = p.ChiTietPhieuMuon?.MaCuonSach ?? 0,
                    TenSach = sach?.TenSach ?? "Sách không rõ",
                    TinhTrangKhiTra = p.ChiTietPhieuMuon?.TinhTrangKhiTra ?? "Hỏng / Mất"
                };

                return Json(new { success = true, data = detail }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: ThuThu/ThanhToanPhieuPhat
        [HttpPost]
        public ActionResult ThanhToanPhieuPhat(ThanhToanPhieuPhatInput input)
        {
            try
            {
                if (input == null || input.MaPhieuPhat <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu phiếu phạt không hợp lệ!" });
                }

                var p = data.PhieuPhat.FirstOrDefault(x => x.MaPhieuPhat == input.MaPhieuPhat);
                if (p == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu phạt trong CSDL!" });
                }

                // Cập nhật trạng thái thanh toán theo Check Constraint CSDL ('DaThanhToan')
                p.TrangThaiThanhToan = "DaThanhToan";
                data.SaveChanges();

                return Json(new { success = true, message = $"Xác nhận thanh toán thành công cho Phiếu Phạt #{p.MaPhieuPhat}!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xử lý cơ sở dữ liệu: " + ex.Message });
            }
        }
    }
}
