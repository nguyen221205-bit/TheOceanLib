using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DoAn_LTWeb.Models;
using DoAn_LTWeb.Models.DTOs;

namespace DoAn_LTWeb.Controllers
{
    public class AdminController : Controller
    {
        private QuanLyThuVienEntities data = new QuanLyThuVienEntities();

        // Filter phân quyền tập trung cho Admin (MaVaiTro == 1)
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                filterContext.Result = RedirectToAction("DangNhap", "Home");
            }
            else if (user.MaVaiTro != 1) // 1 là Admin
            {
                // Nếu không phải Admin thì chuyển hướng về đúng giao diện tương ứng
                if (user.MaVaiTro == 2)
                {
                    filterContext.Result = RedirectToAction("Dashboard", "ThuThu");
                }
                else
                {
                    filterContext.Result = RedirectToAction("Index", "Home");
                }
            }
        }

        // GET: Admin/Index (Dashboard Admin)
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Dashboard";
            ViewBag.Title = "Bảng điều khiển quản trị";

            try
            {
                ViewBag.TotalBooks = data.Sach.Count();
                ViewBag.TotalCopies = data.CuonSach.Count();
                ViewBag.TotalUsers = data.NguoiDung.Count();
                ViewBag.TotalFines = data.PhieuPhat.Where(p => p.TrangThaiThanhToan == "DaThanhToan").Sum(p => (decimal?)p.SoTienPhat) ?? 0;

                return View();
            }
            catch (Exception)
            {
                ViewBag.TotalBooks = 0;
                ViewBag.TotalCopies = 0;
                ViewBag.TotalUsers = 0;
                ViewBag.TotalFines = 0;
                return View();
            }
        }

        #region --- QUẢN LÝ SÁCH & KHO BẢN SÁCH ---

        // GET: Admin/QuanLySach
        public ActionResult QuanLySach(int? maTheLoai, int? maTacGia, int? maNXB, string tuKhoa)
        {
            ViewBag.ActiveMenu = "QuanLySach";
            ViewBag.Title = "Quản lý đầu sách & kho bản sách";

            // Load dropdown items
            ViewBag.DanhSachTheLoai = data.TheLoai.OrderBy(tl => tl.TenTheLoai).ToList();
            ViewBag.DanhSachTacGia = data.TacGia.OrderBy(tg => tg.TenTacGia).ToList();
            ViewBag.DanhSachNXB = data.NhaXuatBan.OrderBy(nxb => nxb.TenNXB).ToList();

            var query = data.Sach
                .Include(s => s.TacGia)
                .Include(s => s.TheLoai)
                .Include(s => s.NhaXuatBan)
                .Include(s => s.CuonSach)
                .AsQueryable();

            if (maTheLoai.HasValue && maTheLoai > 0)
            {
                query = query.Where(s => s.MaTheLoai == maTheLoai.Value);
                ViewBag.SelectedTheLoai = maTheLoai.Value;
            }

            if (maTacGia.HasValue && maTacGia > 0)
            {
                query = query.Where(s => s.MaTacGia == maTacGia.Value);
                ViewBag.SelectedTacGia = maTacGia.Value;
            }

            if (maNXB.HasValue && maNXB > 0)
            {
                query = query.Where(s => s.MaNXB == maNXB.Value);
                ViewBag.SelectedNXB = maNXB.Value;
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string kw = tuKhoa.Trim().ToLower();
                query = query.Where(s => (s.TenSach != null && s.TenSach.ToLower().Contains(kw)) ||
                                         (s.ISBN != null && s.ISBN.ToLower().Contains(kw)) ||
                                         (s.TacGia != null && s.TacGia.TenTacGia.ToLower().Contains(kw)));
                ViewBag.TuKhoa = tuKhoa.Trim();
            }

            List<Sach> dsSach = query.OrderByDescending(s => s.MaSach).ToList();
            return View(dsSach);
        }

        // POST: Admin/ThemSach
        [HttpPost]
        public ActionResult ThemSach(Sach model, HttpPostedFileBase fileAnhBia, int? soLuongBanSaoBanDau, string viTriKeBanDau)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.TenSach))
                {
                    return Json(new { success = false, message = "Vui lòng nhập tên sách!" });
                }

                // Xử lý upload ảnh bìa nếu có
                if (fileAnhBia != null && fileAnhBia.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(fileAnhBia.FileName);
                    string extension = System.IO.Path.GetExtension(fileAnhBia.FileName);
                    string newFileName = $"{fileName}_{DateTime.Now.Ticks}{extension}";
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/HinhAnh/"), newFileName);

                    string dir = Server.MapPath("~/Content/HinhAnh/");
                    if (!System.IO.Directory.Exists(dir))
                    {
                        System.IO.Directory.CreateDirectory(dir);
                    }

                    fileAnhBia.SaveAs(path);
                    model.AnhBia = newFileName;
                }
                else if (string.IsNullOrWhiteSpace(model.AnhBia))
                {
                    model.AnhBia = "default_book.jpg";
                }

                data.Sach.Add(model);
                data.SaveChanges();

                // Tạo các bản sách vật lý ban đầu nếu được nhập số lượng > 0
                int qty = soLuongBanSaoBanDau ?? 0;
                if (qty > 0)
                {
                    string ke = string.IsNullOrWhiteSpace(viTriKeBanDau) ? "Kệ A1" : viTriKeBanDau.Trim();
                    for (int i = 0; i < qty; i++)
                    {
                        CuonSach cs = new CuonSach
                        {
                            MaSach = model.MaSach,
                            TrangThai = "Có sẵn",
                            ViTriKe = ke
                        };
                        data.CuonSach.Add(cs);
                    }
                    data.SaveChanges();
                }

                return Json(new { success = true, message = $"Thêm đầu sách '{model.TenSach}' thành công với {qty} cuốn sách vật lý!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống khi thêm sách: " + ex.Message });
            }
        }

        // GET: Admin/GetChiTietSach/5
        [HttpGet]
        public ActionResult GetChiTietSach(int id)
        {
            var sach = data.Sach.FirstOrDefault(s => s.MaSach == id);
            if (sach == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin đầu sách!" }, JsonRequestBehavior.AllowGet);
            }

            var result = new
            {
                sach.MaSach,
                sach.ISBN,
                sach.TenSach,
                sach.MaTheLoai,
                sach.MaTacGia,
                sach.MaNXB,
                sach.NamXuatBan,
                sach.AnhBia,
                sach.MoTa,
                sach.GiaBia
            };

            return Json(new { success = true, sach = result }, JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/SuaSach
        [HttpPost]
        public ActionResult SuaSach(Sach model, HttpPostedFileBase fileAnhBia)
        {
            try
            {
                var sachInDb = data.Sach.FirstOrDefault(s => s.MaSach == model.MaSach);
                if (sachInDb == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đầu sách cần chỉnh sửa!" });
                }

                if (string.IsNullOrWhiteSpace(model.TenSach))
                {
                    return Json(new { success = false, message = "Tên sách không được để trống!" });
                }

                sachInDb.TenSach = model.TenSach.Trim();
                sachInDb.ISBN = model.ISBN != null ? model.ISBN.Trim() : null;
                sachInDb.MaTheLoai = model.MaTheLoai;
                sachInDb.MaTacGia = model.MaTacGia;
                sachInDb.MaNXB = model.MaNXB;
                sachInDb.NamXuatBan = model.NamXuatBan;
                sachInDb.MoTa = model.MoTa;
                sachInDb.GiaBia = model.GiaBia;

                // Xử lý đổi ảnh bìa mới
                if (fileAnhBia != null && fileAnhBia.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(fileAnhBia.FileName);
                    string extension = System.IO.Path.GetExtension(fileAnhBia.FileName);
                    string newFileName = $"{fileName}_{DateTime.Now.Ticks}{extension}";
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/HinhAnh/"), newFileName);

                    string dir = Server.MapPath("~/Content/HinhAnh/");
                    if (!System.IO.Directory.Exists(dir))
                    {
                        System.IO.Directory.CreateDirectory(dir);
                    }

                    fileAnhBia.SaveAs(path);
                    sachInDb.AnhBia = newFileName;
                }
                else if (!string.IsNullOrWhiteSpace(model.AnhBia))
                {
                    sachInDb.AnhBia = model.AnhBia.Trim();
                }

                data.SaveChanges();
                return Json(new { success = true, message = $"Cập nhật đầu sách '{sachInDb.TenSach}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống khi cập nhật sách: " + ex.Message });
            }
        }

        // POST: Admin/XoaSach
        [HttpPost]
        public ActionResult XoaSach(int id)
        {
            try
            {
                var sach = data.Sach.Include(s => s.CuonSach).FirstOrDefault(s => s.MaSach == id);
                if (sach == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đầu sách cần xóa!" });
                }

                // Kiểm tra xem có cuốn sách nào đang được cho mượn không
                bool hasBorrowedCopies = sach.CuonSach.Any(cs => cs.TrangThai != null && cs.TrangThai.Equals("Đang mượn", StringComparison.OrdinalIgnoreCase));
                if (hasBorrowedCopies)
                {
                    return Json(new { success = false, message = "Không thể xóa đầu sách này vì đang có cuốn sách vật lý ở trạng thái 'Đang mượn'!" });
                }

                // Xóa tất cả cuốn sách vật lý thuộc đầu sách này
                var listCuon = sach.CuonSach.ToList();
                foreach (var cs in listCuon)
                {
                    data.CuonSach.Remove(cs);
                }

                data.Sach.Remove(sach);
                data.SaveChanges();

                return Json(new { success = true, message = $"Đã xóa thành công đầu sách '{sach.TenSach}' và toàn bộ bản sách vật lý liên quan!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Không thể xóa đầu sách do có liên kết dữ liệu mượn/trả trong lịch sử! Chi tiết: " + ex.Message });
            }
        }

        #endregion

        #region --- QUẢN LÝ CUỐN SÁCH VẬT LÝ & VỊ TRÍ KỆ ---

        // GET: Admin/GetDanhSachCuonSach?maSach=5
        [HttpGet]
        public ActionResult GetDanhSachCuonSach(int maSach)
        {
            var sach = data.Sach.FirstOrDefault(s => s.MaSach == maSach);
            if (sach == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đầu sách!" }, JsonRequestBehavior.AllowGet);
            }

            var list = data.CuonSach
                .Where(c => c.MaSach == maSach)
                .OrderBy(c => c.MaCuonSach)
                .ToList()
                .Select(c => new
                {
                    c.MaCuonSach,
                    MaCuonSachFormatted = $"#CS{c.MaCuonSach:D4}",
                    c.MaSach,
                    TrangThai = c.TrangThai ?? "Có sẵn",
                    ViTriKe = c.ViTriKe ?? "Chưa xếp kệ"
                })
                .ToList();

            return Json(new { 
                success = true, 
                tenSach = sach.TenSach,
                maSach = sach.MaSach,
                isbn = sach.ISBN ?? "Chưa có",
                tongSoLuong = list.Count,
                coSan = list.Count(c => c.TrangThai.Equals("Có sẵn", StringComparison.OrdinalIgnoreCase)),
                dangMuon = list.Count(c => c.TrangThai.Equals("Đang mượn", StringComparison.OrdinalIgnoreCase)),
                hongMat = list.Count(c => c.TrangThai.Equals("Hỏng", StringComparison.OrdinalIgnoreCase) || c.TrangThai.Equals("Mất", StringComparison.OrdinalIgnoreCase)),
                list 
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/ThemCuonSach
        [HttpPost]
        public ActionResult ThemCuonSach(int maSach, int soLuong, string viTriKe, string trangThai)
        {
            try
            {
                var sach = data.Sach.FirstOrDefault(s => s.MaSach == maSach);
                if (sach == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đầu sách!" });
                }

                if (soLuong <= 0)
                {
                    soLuong = 1;
                }

                string ke = string.IsNullOrWhiteSpace(viTriKe) ? "Kệ A1" : viTriKe.Trim();
                string status = string.IsNullOrWhiteSpace(trangThai) ? "Có sẵn" : trangThai.Trim();

                for (int i = 0; i < soLuong; i++)
                {
                    CuonSach cs = new CuonSach
                    {
                        MaSach = maSach,
                        ViTriKe = ke,
                        TrangThai = status
                    };
                    data.CuonSach.Add(cs);
                }

                data.SaveChanges();
                return Json(new { success = true, message = $"Đã bổ sung thành công {soLuong} cuốn sách vật lý vào {ke}!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thêm cuốn sách: " + ex.Message });
            }
        }

        // POST: Admin/CapNhatCuonSach
        [HttpPost]
        public ActionResult CapNhatCuonSach(int maCuonSach, string viTriKe, string trangThai)
        {
            try
            {
                var cs = data.CuonSach.FirstOrDefault(c => c.MaCuonSach == maCuonSach);
                if (cs == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy cuốn sách vật lý!" });
                }

                cs.ViTriKe = !string.IsNullOrWhiteSpace(viTriKe) ? viTriKe.Trim() : cs.ViTriKe;
                if (!string.IsNullOrWhiteSpace(trangThai))
                {
                    cs.TrangThai = trangThai.Trim();
                }

                data.SaveChanges();
                return Json(new { success = true, message = $"Đã cập nhật vị trí kệ & trạng thái cho bản sách #CS{maCuonSach:D4}!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật bản sách: " + ex.Message });
            }
        }

        // POST: Admin/XoaCuonSach
        [HttpPost]
        public ActionResult XoaCuonSach(int maCuonSach)
        {
            try
            {
                var cs = data.CuonSach.FirstOrDefault(c => c.MaCuonSach == maCuonSach);
                if (cs == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy cuốn sách vật lý!" });
                }

                if (cs.TrangThai != null && cs.TrangThai.Equals("Đang mượn", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Không thể xóa bản sách đang ở trạng thái 'Đang mượn'!" });
                }

                data.CuonSach.Remove(cs);
                data.SaveChanges();

                return Json(new { success = true, message = $"Đã xóa bản sách vật lý #CS{maCuonSach:D4} khỏi kho!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa bản sách: " + ex.Message });
            }
        }

        #endregion

        #region --- QUẢN LÝ THỂ LOẠI ---

        // GET: Admin/QuanLyTheLoai
        public ActionResult QuanLyTheLoai(string tuKhoa)
        {
            ViewBag.ActiveMenu = "QuanLyTheLoai";
            ViewBag.Title = "Quản lý danh mục Thể loại";

            var query = data.TheLoai.Include(tl => tl.Sach).AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string kw = tuKhoa.Trim().ToLower();
                query = query.Where(tl => tl.TenTheLoai.ToLower().Contains(kw));
                ViewBag.TuKhoa = tuKhoa.Trim();
            }

            var list = query.OrderBy(tl => tl.TenTheLoai).ToList();
            return View(list);
        }

        // POST: Admin/ThemTheLoai
        [HttpPost]
        public ActionResult ThemTheLoai(string tenTheLoai)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenTheLoai))
                {
                    return Json(new { success = false, message = "Vui lòng nhập tên thể loại!" });
                }

                string name = tenTheLoai.Trim();
                if (data.TheLoai.Any(tl => tl.TenTheLoai.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { success = false, message = $"Thể loại '{name}' đã tồn tại trong hệ thống!" });
                }

                TheLoai tlNew = new TheLoai { TenTheLoai = name };
                data.TheLoai.Add(tlNew);
                data.SaveChanges();

                return Json(new { success = true, message = $"Thêm thể loại '{name}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thêm thể loại: " + ex.Message });
            }
        }

        // POST: Admin/SuaTheLoai
        [HttpPost]
        public ActionResult SuaTheLoai(int maTheLoai, string tenTheLoai)
        {
            try
            {
                var tl = data.TheLoai.FirstOrDefault(x => x.MaTheLoai == maTheLoai);
                if (tl == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thể loại cần sửa!" });
                }

                if (string.IsNullOrWhiteSpace(tenTheLoai))
                {
                    return Json(new { success = false, message = "Tên thể loại không được để trống!" });
                }

                string name = tenTheLoai.Trim();
                if (data.TheLoai.Any(x => x.MaTheLoai != maTheLoai && x.TenTheLoai.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { success = false, message = $"Tên thể loại '{name}' đã trùng với thể loại khác!" });
                }

                tl.TenTheLoai = name;
                data.SaveChanges();

                return Json(new { success = true, message = "Cập nhật thể loại thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi sửa thể loại: " + ex.Message });
            }
        }

        // POST: Admin/XoaTheLoai
        [HttpPost]
        public ActionResult XoaTheLoai(int maTheLoai)
        {
            try
            {
                var tl = data.TheLoai.Include(x => x.Sach).FirstOrDefault(x => x.MaTheLoai == maTheLoai);
                if (tl == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thể loại cần xóa!" });
                }

                int countSach = tl.Sach != null ? tl.Sach.Count : 0;
                if (countSach > 0)
                {
                    return Json(new { success = false, message = $"Không thể xóa thể loại '{tl.TenTheLoai}' vì đang có {countSach} đầu sách thuộc thể loại này!" });
                }

                data.TheLoai.Remove(tl);
                data.SaveChanges();

                return Json(new { success = true, message = $"Đã xóa thể loại '{tl.TenTheLoai}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Không thể xóa thể loại do có ràng buộc dữ liệu: " + ex.Message });
            }
        }

        #endregion

        #region --- QUẢN LÝ TÁC GIẢ ---

        // GET: Admin/QuanLyTacGia
        public ActionResult QuanLyTacGia(string tuKhoa)
        {
            ViewBag.ActiveMenu = "QuanLyTacGia";
            ViewBag.Title = "Quản lý danh mục Tác giả";

            var query = data.TacGia.Include(tg => tg.Sach).AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string kw = tuKhoa.Trim().ToLower();
                query = query.Where(tg => tg.TenTacGia.ToLower().Contains(kw));
                ViewBag.TuKhoa = tuKhoa.Trim();
            }

            var list = query.OrderBy(tg => tg.TenTacGia).ToList();
            return View(list);
        }

        // POST: Admin/ThemTacGia
        [HttpPost]
        public ActionResult ThemTacGia(string tenTacGia)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenTacGia))
                {
                    return Json(new { success = false, message = "Vui lòng nhập tên tác giả!" });
                }

                string name = tenTacGia.Trim();
                if (data.TacGia.Any(tg => tg.TenTacGia.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { success = false, message = $"Tác giả '{name}' đã có sẵn trong danh sách!" });
                }

                TacGia tgNew = new TacGia { TenTacGia = name };
                data.TacGia.Add(tgNew);
                data.SaveChanges();

                return Json(new { success = true, message = $"Thêm tác giả '{name}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thêm tác giả: " + ex.Message });
            }
        }

        // POST: Admin/SuaTacGia
        [HttpPost]
        public ActionResult SuaTacGia(int maTacGia, string tenTacGia)
        {
            try
            {
                var tg = data.TacGia.FirstOrDefault(x => x.MaTacGia == maTacGia);
                if (tg == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tác giả cần sửa!" });
                }

                if (string.IsNullOrWhiteSpace(tenTacGia))
                {
                    return Json(new { success = false, message = "Tên tác giả không được để trống!" });
                }

                string name = tenTacGia.Trim();
                if (data.TacGia.Any(x => x.MaTacGia != maTacGia && x.TenTacGia.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { success = false, message = $"Tên tác giả '{name}' đã trùng với tác giả khác!" });
                }

                tg.TenTacGia = name;
                data.SaveChanges();

                return Json(new { success = true, message = "Cập nhật thông tin tác giả thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi sửa tác giả: " + ex.Message });
            }
        }

        // POST: Admin/XoaTacGia
        [HttpPost]
        public ActionResult XoaTacGia(int maTacGia)
        {
            try
            {
                var tg = data.TacGia.Include(x => x.Sach).FirstOrDefault(x => x.MaTacGia == maTacGia);
                if (tg == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tác giả cần xóa!" });
                }

                int countSach = tg.Sach != null ? tg.Sach.Count : 0;
                if (countSach > 0)
                {
                    return Json(new { success = false, message = $"Không thể xóa tác giả '{tg.TenTacGia}' vì đang có {countSach} đầu sách thuộc tác giả này!" });
                }

                data.TacGia.Remove(tg);
                data.SaveChanges();

                return Json(new { success = true, message = $"Đã xóa tác giả '{tg.TenTacGia}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Không thể xóa tác giả do có ràng buộc dữ liệu: " + ex.Message });
            }
        }

        #endregion

        #region --- QUẢN LÝ NHÀ XUẤT BẢN ---

        // GET: Admin/QuanLyNXB
        public ActionResult QuanLyNXB(string tuKhoa)
        {
            ViewBag.ActiveMenu = "QuanLyNXB";
            ViewBag.Title = "Quản lý Nhà xuất bản";

            var query = data.NhaXuatBan.Include(nxb => nxb.Sach).AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string kw = tuKhoa.Trim().ToLower();
                query = query.Where(nxb => nxb.TenNXB.ToLower().Contains(kw));
                ViewBag.TuKhoa = tuKhoa.Trim();
            }

            var list = query.OrderBy(nxb => nxb.TenNXB).ToList();
            return View(list);
        }

        // POST: Admin/ThemNXB
        [HttpPost]
        public ActionResult ThemNXB(string tenNXB)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenNXB))
                {
                    return Json(new { success = false, message = "Vui lòng nhập tên nhà xuất bản!" });
                }

                string name = tenNXB.Trim();
                if (data.NhaXuatBan.Any(n => n.TenNXB.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { success = false, message = $"Nhà xuất bản '{name}' đã có sẵn trong danh sách!" });
                }

                NhaXuatBan nxbNew = new NhaXuatBan { TenNXB = name };
                data.NhaXuatBan.Add(nxbNew);
                data.SaveChanges();

                return Json(new { success = true, message = $"Thêm NXB '{name}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thêm NXB: " + ex.Message });
            }
        }

        // POST: Admin/SuaNXB
        [HttpPost]
        public ActionResult SuaNXB(int maNXB, string tenNXB)
        {
            try
            {
                var nxb = data.NhaXuatBan.FirstOrDefault(x => x.MaNXB == maNXB);
                if (nxb == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy NXB cần sửa!" });
                }

                if (string.IsNullOrWhiteSpace(tenNXB))
                {
                    return Json(new { success = false, message = "Tên nhà xuất bản không được để trống!" });
                }

                string name = tenNXB.Trim();
                if (data.NhaXuatBan.Any(x => x.MaNXB != maNXB && x.TenNXB.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { success = false, message = $"Tên NXB '{name}' đã trùng với NXB khác!" });
                }

                nxb.TenNXB = name;
                data.SaveChanges();

                return Json(new { success = true, message = "Cập nhật thông tin NXB thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi sửa NXB: " + ex.Message });
            }
        }

        // POST: Admin/XoaNXB
        [HttpPost]
        public ActionResult XoaNXB(int maNXB)
        {
            try
            {
                var nxb = data.NhaXuatBan.Include(x => x.Sach).FirstOrDefault(x => x.MaNXB == maNXB);
                if (nxb == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy NXB cần xóa!" });
                }

                int countSach = nxb.Sach != null ? nxb.Sach.Count : 0;
                if (countSach > 0)
                {
                    return Json(new { success = false, message = $"Không thể xóa NXB '{nxb.TenNXB}' vì đang có {countSach} đầu sách thuộc NXB này!" });
                }

                data.NhaXuatBan.Remove(nxb);
                data.SaveChanges();

                return Json(new { success = true, message = $"Đã xóa NXB '{nxb.TenNXB}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Không thể xóa NXB do có ràng buộc dữ liệu: " + ex.Message });
            }
        }

        #endregion

        #region --- QUẢN LÝ TÀI KHOẢN & PHÂN QUYỀN ---

        // GET: Admin/QuanLyTaiKhoan
        public ActionResult QuanLyTaiKhoan(int? maVaiTro, string tuKhoa)
        {
            ViewBag.ActiveMenu = "QuanLyTaiKhoan";
            ViewBag.Title = "Quản lý tài khoản & phân quyền";

            var query = data.NguoiDung.Include(n => n.VaiTro).AsQueryable();

            if (maVaiTro.HasValue && maVaiTro > 0)
            {
                query = query.Where(n => n.MaVaiTro == maVaiTro.Value);
                ViewBag.SelectedVaiTro = maVaiTro.Value;
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string kw = tuKhoa.Trim().ToLower();
                query = query.Where(n => (n.HoTen != null && n.HoTen.ToLower().Contains(kw)) ||
                                         (n.Email != null && n.Email.ToLower().Contains(kw)) ||
                                         (n.SoDienThoai != null && n.SoDienThoai.ToLower().Contains(kw)));
                ViewBag.TuKhoa = tuKhoa.Trim();
            }

            var list = query.OrderBy(n => n.MaVaiTro).ThenByDescending(n => n.MaNguoiDung).ToList();
            ViewBag.DanhSachVaiTro = data.VaiTro.ToList();
            return View(list);
        }

        // POST: Admin/TaoTaiKhoanThuThu
        [HttpPost]
        public ActionResult TaoTaiKhoanThuThu(string hoTen, string email, string matKhau, string soDienThoai, string diaChi)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(matKhau))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ Họ tên, Email và Mật khẩu!" });
                }

                string mail = email.Trim().ToLower();
                if (data.NguoiDung.Any(u => u.Email.ToLower() == mail))
                {
                    return Json(new { success = false, message = $"Email '{mail}' đã tồn tại trên hệ thống!" });
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(matKhau.Trim(), 12);

                NguoiDung newThuThu = new NguoiDung
                {
                    HoTen = hoTen.Trim(),
                    Email = mail,
                    MatKhau = hashedPassword,
                    SoDienThoai = !string.IsNullOrWhiteSpace(soDienThoai) ? soDienThoai.Trim() : null,
                    DiaChi = !string.IsNullOrWhiteSpace(diaChi) ? diaChi.Trim() : null,
                    MaVaiTro = 2, // 2 là Thủ thư
                    TrangThaiThe = "HoatDong",
                    NgayTao = DateTime.Now
                };

                data.NguoiDung.Add(newThuThu);
                data.SaveChanges();

                return Json(new { success = true, message = $"Tạo tài khoản Thủ thư '{newThuThu.HoTen}' ({newThuThu.Email}) thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tạo tài khoản Thủ thư: " + ex.Message });
            }
        }

        // POST: Admin/DoiVaiTro
        [HttpPost]
        public ActionResult DoiVaiTro(int maNguoiDung, int maVaiTroMoi)
        {
            try
            {
                var currentUser = Session["User"] as NguoiDung;
                if (currentUser != null && currentUser.MaNguoiDung == maNguoiDung)
                {
                    return Json(new { success = false, message = "Bạn không thể tự hạ/đổi vai trò của chính tài khoản đang đăng nhập!" });
                }

                var user = data.NguoiDung.FirstOrDefault(u => u.MaNguoiDung == maNguoiDung);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản người dùng!" });
                }

                var vt = data.VaiTro.FirstOrDefault(v => v.MaVaiTro == maVaiTroMoi);
                if (vt == null)
                {
                    return Json(new { success = false, message = "Vai trò không hợp lệ!" });
                }

                user.MaVaiTro = maVaiTroMoi;
                data.SaveChanges();

                return Json(new { success = true, message = $"Đã chuyển đổi vai trò tài khoản '{user.HoTen}' thành '{vt.TenVaiTro}'!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi đổi vai trò tài khoản: " + ex.Message });
            }
        }

        // POST: Admin/DoiTrangThaiTaiKhoan
        [HttpPost]
        public ActionResult DoiTrangThaiTaiKhoan(int maNguoiDung, string trangThaiMoi)
        {
            try
            {
                var currentUser = Session["User"] as NguoiDung;
                if (currentUser != null && currentUser.MaNguoiDung == maNguoiDung)
                {
                    return Json(new { success = false, message = "Bạn không thể tự khóa tài khoản của chính mình!" });
                }

                var user = data.NguoiDung.FirstOrDefault(u => u.MaNguoiDung == maNguoiDung);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản người dùng!" });
                }

                user.TrangThaiThe = trangThaiMoi;
                data.SaveChanges();

                string msg = (trangThaiMoi == "HoatDong" || trangThaiMoi == "Hoạt động") ? "Mở khóa" : "Khóa";
                return Json(new { success = true, message = $"Đã {msg} tài khoản '{user.HoTen}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi đổi trạng thái tài khoản: " + ex.Message });
            }
        }

        // POST: Admin/ResetMatKhau
        [HttpPost]
        public ActionResult ResetMatKhau(int maNguoiDung, string matKhauMoi)
        {
            try
            {
                var user = data.NguoiDung.FirstOrDefault(u => u.MaNguoiDung == maNguoiDung);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản người dùng!" });
                }

                string pass = string.IsNullOrWhiteSpace(matKhauMoi) ? "123456" : matKhauMoi.Trim();
                user.MatKhau = BCrypt.Net.BCrypt.HashPassword(pass, 12);
                data.SaveChanges();

                return Json(new { success = true, message = $"Đã reset mật khẩu cho tài khoản '{user.HoTen}' thành '{pass}'!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi reset mật khẩu: " + ex.Message });
            }
        }

        #endregion

        #region --- BÁO CÁO THỐNG KÊ & DOANH THU ---

        // GET: Admin/BaoCaoThongKe
        public ActionResult BaoCaoThongKe()
        {
            ViewBag.ActiveMenu = "BaoCaoThongKe";
            ViewBag.Title = "Báo cáo thống kê & doanh thu chuyên sâu";

            try
            {
                // 1. Thống kê 12 tháng gần nhất
                List<string> monthLabels = new List<string>();
                List<int> borrowCounts = new List<int>();
                List<decimal> fineRevenue = new List<decimal>();

                DateTime now = DateTime.Now;
                for (int i = 11; i >= 0; i--)
                {
                    DateTime mDate = now.AddMonths(-i);
                    monthLabels.Add($"T{mDate.Month}/{mDate.Year}");

                    // Đếm lượt mượn trong tháng
                    int countBorrow = data.PhieuMuon.Count(p => p.NgayMuon.HasValue &&
                                                               p.NgayMuon.Value.Month == mDate.Month &&
                                                               p.NgayMuon.Value.Year == mDate.Year);
                    borrowCounts.Add(countBorrow);

                    // Doanh thu phạt thực tế đã thu trong tháng
                    decimal revenue = data.PhieuPhat
                        .Where(p => p.TrangThaiThanhToan == "DaThanhToan" &&
                                    p.NgayLap.HasValue &&
                                    p.NgayLap.Value.Month == mDate.Month &&
                                    p.NgayLap.Value.Year == mDate.Year)
                        .Sum(p => (decimal?)p.SoTienPhat) ?? 0;
                    fineRevenue.Add(revenue);
                }

                ViewBag.MonthLabels = Newtonsoft.Json.JsonConvert.SerializeObject(monthLabels);
                ViewBag.BorrowCounts = Newtonsoft.Json.JsonConvert.SerializeObject(borrowCounts);
                ViewBag.FineRevenue = Newtonsoft.Json.JsonConvert.SerializeObject(fineRevenue);

                // 2. Chỉ số tổng quan
                decimal totalCollectedFines = data.PhieuPhat.Where(p => p.TrangThaiThanhToan == "DaThanhToan").Sum(p => (decimal?)p.SoTienPhat) ?? 0;
                decimal totalPendingFines = data.PhieuPhat.Where(p => p.TrangThaiThanhToan != "DaThanhToan").Sum(p => (decimal?)p.SoTienPhat) ?? 0;
                int totalBorrowTickets = data.PhieuMuon.Count();
                int totalUsers = data.NguoiDung.Count();

                ViewBag.TotalCollectedFines = totalCollectedFines;
                ViewBag.TotalPendingFines = totalPendingFines;
                ViewBag.TotalBorrowTickets = totalBorrowTickets;
                ViewBag.TotalUsers = totalUsers;

                // 3. Top 5 thể loại mượn nhiều nhất
                var categoryBorrowStats = data.ChiTietPhieuMuon
                    .GroupBy(ct => ct.CuonSach.Sach.TheLoai.TenTheLoai)
                    .Select(g => new TheLoaiThongKeDto
                    {
                        TenTheLoai = g.Key ?? "Chưa phân loại",
                        SoLuong = g.Count()
                    })
                    .OrderByDescending(x => x.SoLuong)
                    .Take(5)
                    .ToList();

                ViewBag.CategoryLabels = Newtonsoft.Json.JsonConvert.SerializeObject(categoryBorrowStats.Select(x => x.TenTheLoai).ToArray());
                ViewBag.CategoryCounts = Newtonsoft.Json.JsonConvert.SerializeObject(categoryBorrowStats.Select(x => x.SoLuong).ToArray());

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        #endregion
    }
}
