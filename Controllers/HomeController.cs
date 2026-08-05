using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAn_LTWeb.Models;
using DoAn_LTWeb.Models.DTOs;
using System.Data.Entity;
namespace DoAn_LTWeb.Controllers
{
    public class HomeController : Controller
    {
        QuanLyThuVienEntities data = new QuanLyThuVienEntities();
        public ActionResult Index()
        {
            List<Sach> dssp = data.Sach.Include("TacGia").Include("TheLoai").Include("CuonSach").Take(4).ToList();
            ViewBag.DanhSachTheLoai = data.TheLoai.ToList(); // Tải danh mục thực tế từ database
            return View(dssp);
        }

        public ActionResult HienThiDSSP(int? maTheLoai, string tuKhoa, string sort = "moi-nhat", int page = 1)
        {
            var query = data.Sach.Include("TacGia").Include("CuonSach").Include("TheLoai").AsQueryable();

            if (maTheLoai.HasValue)
            {
                query = query.Where(s => s.MaTheLoai == maTheLoai.Value);
                var theLoai = data.TheLoai.FirstOrDefault(tl => tl.MaTheLoai == maTheLoai.Value);
                ViewBag.TenTheLoai = theLoai != null ? theLoai.TenTheLoai : "";
                ViewBag.MaTheLoai = maTheLoai.Value;
            }

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(s => s.TenSach.Contains(tuKhoa) || 
                                         s.MoTa.Contains(tuKhoa) || 
                                         (s.TacGia != null && s.TacGia.TenTacGia.Contains(tuKhoa)));
                ViewBag.TuKhoa = tuKhoa;
            }

            // Sắp xếp
            switch (sort)
            {
                case "ten-az":
                    query = query.OrderBy(s => s.TenSach);
                    break;
                case "ten-za":
                    query = query.OrderByDescending(s => s.TenSach);
                    break;
                case "nam-xb":
                    query = query.OrderByDescending(s => s.NamXuatBan);
                    break;
                default: // "moi-nhat"
                    query = query.OrderByDescending(s => s.MaSach);
                    break;
            }

            ViewBag.Sort = sort;

            // Phân trang
            int pageSize = 8;
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            List<Sach> dssp = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return View(dssp);
        }

        [ChildActionOnly]
        public ActionResult MenuTheLoai()
        {
            var listTheLoai = data.TheLoai.ToList();
            List<MenuTheLoaiCap2Dto> menuList = new List<MenuTheLoaiCap2Dto>();

            foreach (var tl in listTheLoai)
            {
                var dto = new MenuTheLoaiCap2Dto
                {
                    MaTheLoai = tl.MaTheLoai,
                    TenTheLoai = tl.TenTheLoai
                };

                // Lấy danh sách Tác giả tiêu biểu có sách thuộc thể loại này
                var tacGias = data.Sach
                                  .Where(s => s.MaTheLoai == tl.MaTheLoai && s.TacGia != null)
                                  .GroupBy(s => new { s.TacGia.MaTacGia, s.TacGia.TenTacGia })
                                  .Select(g => new TacGiaMenuDto
                                  {
                                      MaTacGia = g.Key.MaTacGia,
                                      TenTacGia = g.Key.TenTacGia,
                                      SoLuongSach = g.Count()
                                  })
                                  .Take(5)
                                  .ToList();

                dto.DanhSachTacGia = tacGias;

                // Lấy 3 cuốn sách nổi bật thuộc thể loại này
                var sachs = data.Sach
                                .Where(s => s.MaTheLoai == tl.MaTheLoai)
                                .OrderByDescending(s => s.MaSach)
                                .Select(s => new SachMenuDto
                                {
                                    MaSach = s.MaSach,
                                    TenSach = s.TenSach,
                                    AnhBia = s.AnhBia
                                })
                                .Take(3)
                                .ToList();

                dto.DanhSachSachNoiBat = sachs;

                menuList.Add(dto);
            }

            return PartialView("_MenuTheLoai", menuList);
        }

        public ActionResult ChiTietSach(int id)
        {
            var sach = data.Sach.Include("CuonSach").FirstOrDefault(s => s.MaSach == id);
            if (sach == null)
            {
                return HttpNotFound();
            }

            ViewBag.TenTacGia = data.TacGia
                                    .Where(t => t.MaTacGia == sach.MaTacGia)
                                    .Select(t => t.TenTacGia)
                                    .FirstOrDefault();

            // Lấy danh sách sản phẩm liên quan
            // Ưu tiên 1: Cùng tác giả VÀ cùng thể loại (trừ chính nó)
            var dsLienQuan = data.Sach.Include("TacGia")
                                 .Where(s => s.MaSach != id && s.MaTacGia == sach.MaTacGia && s.MaTheLoai == sach.MaTheLoai)
                                 .Take(4)
                                 .ToList();

            // Ưu tiên 2: Cùng tác giả HOẶC cùng thể loại
            if (dsLienQuan.Count < 4)
            {
                var idsDaLay = dsLienQuan.Select(s => s.MaSach).ToList();
                idsDaLay.Add(id); // Loại trừ cả sách hiện tại

                var layThem = data.Sach.Include("TacGia")
                                  .Where(s => !idsDaLay.Contains(s.MaSach) && (s.MaTacGia == sach.MaTacGia || s.MaTheLoai == sach.MaTheLoai))
                                  .Take(4 - dsLienQuan.Count)
                                  .ToList();
                dsLienQuan.AddRange(layThem);
            }

            ViewBag.DanhSachLienQuan = dsLienQuan;

            return View(sach);
        }

        public GioHang LayGioHang()
        {
            GioHang gh = Session["GioHang"] as GioHang;

            if (gh == null)
            {
                gh = new GioHang();
                Session["GioHang"] = gh;
            }

            return gh;
        }

        [HttpPost]
        public ActionResult ChonSach(int id, int? soLuong)
        {
            int qty = soLuong ?? 1;

            // 1. Tìm sách trong Database theo id truyền vào
            Sach sach = data.Sach.FirstOrDefault(s => s.MaSach == id);

            if (sach == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sách!" }, JsonRequestBehavior.AllowGet);
            }

            // 2. Lấy giỏ hàng từ Session (hoặc tạo mới nếu chưa có)
            GioHang gioHang = Session["GioHang"] as GioHang;
            if (gioHang == null)
            {
                gioHang = new GioHang();
            }

            // 3. Thêm sách với số lượng tương ứng
            CartItem item = gioHang.lst.FirstOrDefault(x => x.iMaSach == id);
            if (item == null)
            {
                CartItem newItem = new CartItem(sach);
                newItem.iSoLuong = qty;
                gioHang.lst.Add(newItem);
            }
            else
            {
                item.iSoLuong += qty;
            }

            // 4. Lưu lại giỏ hàng vào Session
            Session["GioHang"] = gioHang;

            // 5. Lấy tổng số lượng bằng phương thức TongSLHang()
            int tongSoLuong = gioHang.TongSLHang();

            // 6. Trả về JSON cho AJAX
            return Json(new { success = true, totalCount = tongSoLuong }, JsonRequestBehavior.AllowGet);
        }

        // GET: ChonSachVaQuayLai
        public ActionResult ChonSachVaQuayLai(int id)
        {
            Sach sach = data.Sach.FirstOrDefault(s => s.MaSach == id);
            if (sach != null)
            {
                GioHang gioHang = Session["GioHang"] as GioHang;
                if (gioHang == null)
                {
                    gioHang = new GioHang();
                }
                gioHang.Them(sach);
                Session["GioHang"] = gioHang;
            }
            return RedirectToAction("HienThiDSSP");
        }

        [HttpPost]
        public ActionResult ThemDanhSachVaoGioAjax(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return Json(new { success = false, message = "Danh sách sách trống!" });
            }

            GioHang gioHang = Session["GioHang"] as GioHang;
            if (gioHang == null)
            {
                gioHang = new GioHang();
            }

            int addedCount = 0;
            foreach (var id in ids)
            {
                // Giới hạn tối đa 5 quyển sách khác nhau trong giỏ mượn
                var daCo = gioHang.lst.Any(x => x.iMaSach == id);
                if (!daCo && gioHang.lst.Count < 5)
                {
                    var sach = data.Sach.FirstOrDefault(s => s.MaSach == id);
                    if (sach != null)
                    {
                        gioHang.Them(sach);
                        addedCount++;
                    }
                }
            }

            Session["GioHang"] = gioHang;
            return Json(new { success = true, addedCount = addedCount, totalQty = gioHang.TongSLHang() });
        }


        // GET: XemGioHang
        public ActionResult XemGioHang()
        {
            // 1. Lấy giỏ hàng từ Session
            GioHang gioHang = Session["GioHang"] as GioHang;

            // 2. Nếu chưa có giỏ hàng, khởi tạo giỏ hàng rỗng để tránh lỗi null
            if (gioHang == null)
            {
                gioHang = new GioHang();
            }

            // 3. Truyền trực tiếp đối tượng gioHang sang View
            return View(gioHang);
        }

        public ActionResult XoaGioHang(int id)
        {
            GioHang gioHang = Session["GioHang"] as GioHang;
            if (gioHang != null)
            {
                gioHang.Xoa(id); // Sử dụng hàm Xoa() đã có sẵn trong class GioHang
                Session["GioHang"] = gioHang;
            }
            return RedirectToAction("XemGioHang");
        }

        [HttpPost]
        public ActionResult CapNhatGioHang(int id, int soLuong)
        {
            GioHang gioHang = Session["GioHang"] as GioHang;

            if (gioHang != null)
            {
                // Ràng buộc số lượng không vượt quá giới hạn (ví dụ tối đa 5 cuốn)
                if (soLuong > 0 && soLuong <= 5)
                {
                    gioHang.CapNhat(id, soLuong);
                    Session["GioHang"] = gioHang;
                }
            }

            return RedirectToAction("XemGioHang");
        }

        [HttpPost]
        public ActionResult CapNhatSoLuongAjax(int id, int change)
        {
            GioHang gioHang = Session["GioHang"] as GioHang;
            int newQty = 0;
            int totalQty = 0;
            bool removed = false;

            if (gioHang != null)
            {
                var item = gioHang.lst.FirstOrDefault(x => x.iMaSach == id);
                if (item != null)
                {
                    int targetQty = item.iSoLuong + change;
                    if (targetQty <= 0)
                    {
                        gioHang.Xoa(id);
                        removed = true;
                    }
                    else if (targetQty <= 5) // limit max 5
                    {
                        gioHang.CapNhat(id, targetQty);
                        newQty = targetQty;
                    }
                    else
                    {
                        newQty = item.iSoLuong; // keep current if > 5
                    }
                    Session["GioHang"] = gioHang;
                    totalQty = gioHang.TongSLHang();
                }
            }

            return Json(new { success = true, newQty = newQty, totalQty = totalQty, removed = removed });
        }

        // GET: XacNhanMuon
        public ActionResult XacNhanMuon()
        {
            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                return RedirectToAction("DangNhap", "Home", new { returnUrl = Url.Action("XacNhanMuon") });
            }

            GioHang gioHang = Session["GioHang"] as GioHang;
            if (gioHang == null || gioHang.TongSLHang() == 0)
            {
                return RedirectToAction("XemGioHang");
            }
            ViewBag.User = user;
            return View(gioHang);
        }

        // POST: XacNhanMuon
        [HttpPost]
        public ActionResult XacNhanMuon(FormCollection form)
        {
            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                return RedirectToAction("DangNhap", "Home");
            }

            GioHang gioHang = Session["GioHang"] as GioHang;
            if (gioHang == null || gioHang.TongSLHang() == 0)
            {
                return RedirectToAction("XemGioHang");
            }

            // Lấy thông tin từ form
            string hoTen = form["HoTen"];
            string ngayMuon = form["NgayMuon"];
            string ngayTra = form["NgayTra"];
            string sdt = form["SoDienThoai"];
            string diaChi = form["DiaChi"];

            // ====== BƯỚC 1: Lưu Yêu Cầu Mượn =====
            YeuCauMuon yc = new YeuCauMuon();
            yc.MaDocGia = user.MaNguoiDung;
            yc.NgayYeuCau = DateTime.Now;
            yc.TrangThai = "Chờ duyệt";

            data.YeuCauMuon.Add(yc);
            data.SaveChanges(); // Lưu để lấy MaYeuCau tự tăng

            // ====== BƯỚC 2: Lưu Chi Tiết Yêu Cầu Mượn =====
            foreach (var item in gioHang.lst)
            {
                // Thêm số lượng bản ghi tương ứng với số lượng sách độc giả đăng ký mượn
                for (int i = 0; i < item.iSoLuong; i++)
                {
                    ChiTietYeuCauMuon ct = new ChiTietYeuCauMuon();
                    ct.MaYeuCau = yc.MaYeuCau;
                    ct.MaSach = item.iMaSach;

                    data.ChiTietYeuCauMuon.Add(ct);
                }
            }

            // ====== BƯỚC 3: Cập nhật lại thông tin độc giả nếu có thay đổi =====
            var currentUserDb = data.NguoiDung.Find(user.MaNguoiDung);
            if (currentUserDb != null)
            {
                if (!string.IsNullOrEmpty(sdt)) currentUserDb.SoDienThoai = sdt;
                if (!string.IsNullOrEmpty(diaChi)) currentUserDb.DiaChi = diaChi;
                if (!string.IsNullOrEmpty(hoTen)) currentUserDb.HoTen = hoTen;
            }

            try
            {
                data.SaveChanges();
                if (currentUserDb != null) Session["User"] = currentUserDb;
            }
            catch (Exception ex)
            {
                var err = ex;
                while (err.InnerException != null)
                {
                    err = err.InnerException;
                }
                throw new Exception(err.Message);
            }

            // Làm sạch giỏ hàng sau khi gửi yêu cầu mượn thành công
            Session["GioHang"] = null;

            TempData["SuccessMessage"] = "Gửi yêu cầu mượn sách thành công! Yêu cầu của bạn đã được gửi tới Thủ thư. Vui lòng theo dõi trạng thái và chờ duyệt tại đây.";

            return RedirectToAction("TaiKhoan", "Home", new { tab = "requests" });
        }

        public ActionResult SachMuonNhieu()
        {
            var ds = data.ChiTietPhieuMuon
                         .GroupBy(x => new { 
                             x.CuonSach.Sach.MaSach, 
                             x.CuonSach.Sach.TenSach, 
                             x.CuonSach.Sach.AnhBia,
                             TenTacGia = x.CuonSach.Sach.TacGia != null ? x.CuonSach.Sach.TacGia.TenTacGia : "Chưa cập nhật"
                         })
                         .Select(g => new SachMuonNhieuVM
                         {
                             MaSach = g.Key.MaSach,
                             TenSach = g.Key.TenSach,
                             AnhBia = g.Key.AnhBia,
                             TenTacGia = g.Key.TenTacGia,
                             SoLanMuon = g.Count()
                         })
                         .OrderByDescending(x => x.SoLanMuon)
                         .Take(10)
                         .ToList();

            return View(ds);
        }

        public ActionResult TaiLieuMoi(string tuKhoa, int? maTheLoai)
        {
            var query = data.Sach
                            .Include(s => s.TacGia)
                            .Include(s => s.TheLoai)
                            .Include(s => s.CuonSach)
                            .AsQueryable();

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(s => s.TenSach.Contains(tuKhoa));
            }

            if (maTheLoai.HasValue)
            {
                query = query.Where(s => s.MaTheLoai == maTheLoai);
            }

            ViewBag.DanhSachTheLoai = data.TheLoai.ToList();

            return View(query.OrderByDescending(s => s.MaSach).Take(20).ToList());
        }

        public ActionResult HuongDanMuonTra()
        {
            return View();
        }

        public ActionResult LienHeHoTro()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LienHeHoTro(FormCollection form)
        {
            ViewBag.Message = "Cảm ơn bạn đã gửi phản hồi. Chúng tôi sẽ liên hệ sớm nhất!";
            return View();
        }

        // ================= ĐĂNG NHẬP & ĐĂNG KÝ (REAL SESSION AUTH + BCRYPT) =================

        // GET: Home/DangNhap
        public ActionResult DangNhap(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Home/DangNhap
        [HttpPost]
        public ActionResult DangNhap(string Email, string MatKhau, string returnUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(MatKhau))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ Email và Mật khẩu!" });
                }

                Email = Email.Trim();
                MatKhau = MatKhau.Trim();

                // Tìm người dùng trong database
                var user = data.NguoiDung.Include(n => n.VaiTro).FirstOrDefault(u => u.Email == Email);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản với Email này trong hệ thống!" });
                }

                if (user.TrangThaiThe == "Khoa")
                {
                    return Json(new { success = false, message = "Tài khoản của bạn đã bị khóa! Vui lòng liên hệ thủ thư." });
                }

                // Kiểm tra mật khẩu (BCrypt mã hóa)
                bool isValid = false;
                string debugMsg = "";
                string storedHash = (user.MatKhau ?? "").Trim();

                try
                {
                    isValid = BCrypt.Net.BCrypt.Verify(MatKhau, storedHash);
                }
                catch (Exception ex)
                {
                    debugMsg = $" [Lỗi giải mã: {ex.Message}]";
                }

                if (!isValid)
                {
                    // Fallback trong trường hợp mật khẩu trong CSDL trùng khớp trực tiếp hoặc là plain-text
                    if (MatKhau == storedHash)
                    {
                        isValid = true;
                    }
                }

                if (!isValid)
                {
                    return Json(new { success = false, message = "Mật khẩu nhập vào không chính xác!" + debugMsg });
                }

                // Lưu thông tin vào Session
                Session["User"] = user;

                // Điều hướng dựa vào vai trò
                string redirectUrl = Url.Action("Index", "Home");
                if (user.MaVaiTro == 1 || user.MaVaiTro == 2)
                {
                    redirectUrl = Url.Action("Dashboard", "ThuThu");
                }
                else if (!string.IsNullOrEmpty(returnUrl))
                {
                    redirectUrl = returnUrl;
                }

                return Json(new { success = true, message = $"Chào mừng {user.HoTen} đã đăng nhập thành công!", redirectUrl = redirectUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // POST: Home/DangKy
        [HttpPost]
        public ActionResult DangKy(NguoiDung model, string ConfirmPassword)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.MatKhau) || string.IsNullOrEmpty(model.HoTen) || string.IsNullOrEmpty(model.SoDienThoai))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ các trường thông tin bắt buộc!" });
                }

                if (model.MatKhau.Length < 6)
                {
                    return Json(new { success = false, message = "Mật khẩu phải chứa ít nhất 6 ký tự!" });
                }

                if (model.MatKhau != ConfirmPassword)
                {
                    return Json(new { success = false, message = "Xác nhận mật khẩu không khớp!" });
                }

                // Kiểm tra email trùng
                var existEmail = data.NguoiDung.Any(u => u.Email == model.Email);
                if (existEmail)
                {
                    return Json(new { success = false, message = "Email này đã được đăng ký tài khoản khác!" });
                }

                // Băm mật khẩu bằng BCrypt
                string hashed = BCrypt.Net.BCrypt.HashPassword(model.MatKhau, 12);

                NguoiDung newUser = new NguoiDung
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    MatKhau = hashed,
                    SoDienThoai = model.SoDienThoai,
                    DiaChi = model.DiaChi ?? "",
                    MaVaiTro = 3, // Mặc định là Độc giả (DocGia)
                    TrangThaiThe = "HoatDong", // Mặc định hoạt động
                    NgayTao = DateTime.Now
                };

                data.NguoiDung.Add(newUser);
                data.SaveChanges();

                // Đăng nhập tự động sau khi đăng ký
                Session["User"] = newUser;

                return Json(new { success = true, message = "Đăng ký tài khoản thành công!", redirectUrl = Url.Action("Index", "Home") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xử lý đăng ký: " + ex.Message });
            }
        }

        // GET: Home/DangXuat
        public ActionResult DangXuat()
        {
            Session["User"] = null;
            return RedirectToAction("Index", "Home");
        }

        // ================= PHÂN HỆ ĐỘC GIẢ (READER PORTAL & HISTORY) =================

        // GET: Home/TaiKhoan
        public ActionResult TaiKhoan(string tab = "profile")
        {
            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                return RedirectToAction("DangNhap", "Home", new { returnUrl = Url.Action("TaiKhoan", "Home", new { tab = tab }) });
            }

            var userDb = data.NguoiDung.Find(user.MaNguoiDung);
            if (userDb == null)
            {
                Session["User"] = null;
                return RedirectToAction("DangNhap", "Home");
            }

            Session["User"] = userDb;

            TaiKhoanDocGiaDto model = new TaiKhoanDocGiaDto
            {
                NguoiDung = userDb,
                ActiveTab = string.IsNullOrEmpty(tab) ? "profile" : tab.ToLower()
            };

            // 1. Lấy danh sách Yêu cầu mượn
            var listYeuCau = data.YeuCauMuon
                                 .Include("ChiTietYeuCauMuon.Sach.TacGia")
                                 .Where(y => y.MaDocGia == userDb.MaNguoiDung)
                                 .OrderByDescending(y => y.MaYeuCau)
                                 .ToList();

            foreach (var yc in listYeuCau)
            {
                var dto = new YeuCauMuonDocGiaDto
                {
                    MaYeuCau = yc.MaYeuCau,
                    NgayYeuCau = yc.NgayYeuCau,
                    TrangThai = yc.TrangThai
                };

                var groupedSach = yc.ChiTietYeuCauMuon
                                    .GroupBy(c => c.MaSach)
                                    .Select(g => new ChiTietYeuCauItemDto
                                    {
                                        MaSach = g.Key,
                                        TenSach = g.FirstOrDefault()?.Sach?.TenSach ?? "Chưa rõ",
                                        AnhBia = g.FirstOrDefault()?.Sach?.AnhBia ?? "",
                                        TenTacGia = g.FirstOrDefault()?.Sach?.TacGia?.TenTacGia ?? "Chưa cập nhật",
                                        SoLuong = g.Count()
                                    }).ToList();

                dto.DanhSachSach = groupedSach;
                model.DanhSachYeuCau.Add(dto);
            }

            // 2. Lấy danh sách Phiếu mượn
            var listPhieuMuon = data.PhieuMuon
                                    .Include("ChiTietPhieuMuon.CuonSach.Sach.TacGia")
                                    .Include("NguoiDung1") // Thủ thư cấp
                                    .Where(p => p.MaDocGia == userDb.MaNguoiDung)
                                    .OrderByDescending(p => p.MaPhieuMuon)
                                    .ToList();

            foreach (var pm in listPhieuMuon)
            {
                var dto = new PhieuMuonDocGiaDto
                {
                    MaPhieuMuon = pm.MaPhieuMuon,
                    NgayMuon = pm.NgayMuon,
                    NgayHenTra = pm.NgayHenTra,
                    TrangThai = pm.TrangThai,
                    TenThuThu = pm.NguoiDung1 != null ? pm.NguoiDung1.HoTen : "Hệ thống"
                };

                foreach (var ct in pm.ChiTietPhieuMuon)
                {
                    var sach = ct.CuonSach?.Sach;
                    dto.DanhSachCuonSach.Add(new ChiTietPhieuMuonItemDto
                    {
                        MaChiTiet = ct.MaChiTiet,
                        MaCuonSach = ct.MaCuonSach,
                        TenSach = sach != null ? sach.TenSach : "Bản sách #" + ct.MaCuonSach,
                        AnhBia = sach != null ? sach.AnhBia : "",
                        TenTacGia = sach?.TacGia != null ? sach.TacGia.TenTacGia : "Chưa cập nhật",
                        NgayTraThucTe = ct.NgayTraThucTe,
                        TinhTrangKhiTra = ct.TinhTrangKhiTra
                    });
                }

                model.DanhSachPhieuMuon.Add(dto);
            }

            // 3. Lấy danh sách Phiếu phạt
            var listPhieuPhat = data.PhieuPhat
                                    .Include("ChiTietPhieuMuon.CuonSach.Sach")
                                    .Where(p => p.ChiTietPhieuMuon.PhieuMuon.MaDocGia == userDb.MaNguoiDung)
                                    .OrderByDescending(p => p.MaPhieuPhat)
                                    .ToList();

            foreach (var pp in listPhieuPhat)
            {
                var sach = pp.ChiTietPhieuMuon?.CuonSach?.Sach;
                model.DanhSachPhieuPhat.Add(new PhieuPhatDocGiaDto
                {
                    MaPhieuPhat = pp.MaPhieuPhat,
                    MaPhieuMuon = pp.ChiTietPhieuMuon != null ? pp.ChiTietPhieuMuon.MaPhieuMuon : 0,
                    TenSach = sach != null ? sach.TenSach : "Bản sách #" + (pp.ChiTietPhieuMuon != null ? pp.ChiTietPhieuMuon.MaCuonSach : 0),
                    AnhBia = sach != null ? sach.AnhBia : "",
                    MaCuonSach = pp.ChiTietPhieuMuon != null ? pp.ChiTietPhieuMuon.MaCuonSach : 0,
                    SoTienPhat = pp.SoTienPhat,
                    LyDoPhat = pp.LyDo,
                    NgayLap = pp.NgayLap,
                    TrangThaiThanhToan = pp.TrangThaiThanhToan
                });
            }

            return View(model);
        }

        // POST: Home/CapNhatThongTin
        [HttpPost]
        public ActionResult CapNhatThongTin(CapNhatThongTinInput input)
        {
            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập lại để thực hiện thao tác!" });
            }

            if (input == null || string.IsNullOrWhiteSpace(input.HoTen) || string.IsNullOrWhiteSpace(input.SoDienThoai))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ Họ tên và Số điện thoại!" });
            }

            var userDb = data.NguoiDung.Find(user.MaNguoiDung);
            if (userDb == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin tài khoản trên hệ thống!" });
            }

            userDb.HoTen = input.HoTen.Trim();
            userDb.SoDienThoai = input.SoDienThoai.Trim();
            userDb.DiaChi = (input.DiaChi ?? "").Trim();

            data.SaveChanges();

            Session["User"] = userDb;

            return Json(new { success = true, message = "Cập nhật thông tin cá nhân thành công!" });
        }

        // POST: Home/DoiMatKhau
        [HttpPost]
        public ActionResult DoiMatKhau(DoiMatKhauInput input)
        {
            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập lại để thực hiện thao tác!" });
            }

            if (input == null || string.IsNullOrEmpty(input.MatKhauCu) || string.IsNullOrEmpty(input.MatKhauMoi) || string.IsNullOrEmpty(input.XacNhanMatKhau))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin các trường mật khẩu!" });
            }

            if (input.MatKhauMoi.Length < 6)
            {
                return Json(new { success = false, message = "Mật khẩu mới phải có độ dài tối thiểu 6 ký tự!" });
            }

            if (input.MatKhauMoi != input.XacNhanMatKhau)
            {
                return Json(new { success = false, message = "Mật khẩu mới và Xác nhận mật khẩu không trùng khớp!" });
            }

            var userDb = data.NguoiDung.Find(user.MaNguoiDung);
            if (userDb == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin tài khoản trên hệ thống!" });
            }

            string storedHash = (userDb.MatKhau ?? "").Trim();
            bool isValid = false;

            try
            {
                isValid = BCrypt.Net.BCrypt.Verify(input.MatKhauCu, storedHash);
            }
            catch
            {
                isValid = false;
            }

            if (!isValid && input.MatKhauCu == storedHash)
            {
                isValid = true;
            }

            if (!isValid)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại nhập vào không chính xác!" });
            }

            string hashedNew = BCrypt.Net.BCrypt.HashPassword(input.MatKhauMoi, 12);
            userDb.MatKhau = hashedNew;

            data.SaveChanges();
            Session["User"] = userDb;

            return Json(new { success = true, message = "Đổi mật khẩu thành công! Hãy bảo mật thông tin tài khoản của bạn." });
        }

        // POST: Home/HuyYeuCauMuon
        [HttpPost]
        public ActionResult HuyYeuCauMuon(int id)
        {
            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập lại để thực hiện thao tác!" });
            }

            var yc = data.YeuCauMuon.FirstOrDefault(y => y.MaYeuCau == id && y.MaDocGia == user.MaNguoiDung);
            if (yc == null)
            {
                return Json(new { success = false, message = "Không tìm thấy yêu cầu mượn cần hủy!" });
            }

            if (yc.TrangThai != "Chờ duyệt")
            {
                return Json(new { success = false, message = $"Không thể hủy yêu cầu mượn đang ở trạng thái '{yc.TrangThai}'!" });
            }

            yc.TrangThai = "Đã hủy";
            data.SaveChanges();

            return Json(new { success = true, message = $"Đã hủy yêu cầu mượn #{yc.MaYeuCau} thành công!" });
        }
    }
}
