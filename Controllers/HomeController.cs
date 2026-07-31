using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAn_LTWeb.Models;
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

        public ActionResult HienThiDSSP(int? maTheLoai, string tuKhoa)
        {
            var query = data.Sach.Include("TacGia").Include("CuonSach").AsQueryable();

            if (maTheLoai.HasValue)
            {
                query = query.Where(s => s.MaTheLoai == maTheLoai.Value);
                var theLoai = data.TheLoai.FirstOrDefault(tl => tl.MaTheLoai == maTheLoai.Value);
                ViewBag.TenTheLoai = theLoai != null ? theLoai.TenTheLoai : "";
            }

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(s => s.TenSach.Contains(tuKhoa) || 
                                         s.MoTa.Contains(tuKhoa) || 
                                         (s.TacGia != null && s.TacGia.TenTacGia.Contains(tuKhoa)));
                ViewBag.TuKhoa = tuKhoa;
            }

            List<Sach> dssp = query.ToList();
            return View(dssp);
        }

        [ChildActionOnly]
        public ActionResult MenuTheLoai()
        {
            var listTheLoai = data.TheLoai.ToList();
            return PartialView("_MenuTheLoai", listTheLoai);
        }

        public ActionResult ChiTietSach(int id)
        {
            var sach = data.Sach.Include("CuonSach").FirstOrDefault(s => s.MaSach == id);

            ViewBag.TenTacGia = data.TacGia
                                    .Where(t => t.MaTacGia == sach.MaTacGia)
                                    .Select(t => t.TenTacGia)
                                    .FirstOrDefault();

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
        public ActionResult ChonSach(int id)
        {
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

            // 3. Gọi hàm Them(sach) có sẵn trong model GioHang của bạn
            gioHang.Them(sach);

            // 4. Lưu lại giỏ hàng vào Session
            Session["GioHang"] = gioHang;

            // 5. Lấy tổng số lượng bằng phương thức TongSLHang() có sẵn trong class GioHang
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
            GioHang gioHang = Session["GioHang"] as GioHang;
            if (gioHang == null || gioHang.TongSLHang() == 0)
            {
                return RedirectToAction("XemGioHang");
            }
            return View(gioHang);
        }

        // POST: XacNhanMuon
        [HttpPost]
        public ActionResult XacNhanMuon(FormCollection form)
        {
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

            // ====== BƯỚC 1: Lưu Phiếu Mượn =====
            PhieuMuon pm = new PhieuMuon();

            pm.MaDocGia = 1;
            pm.MaThuThu = null;
            pm.NgayMuon = DateTime.Now;
            pm.NgayHenTra = DateTime.Parse(ngayTra);

            // Lưu ý: Kiểm tra lại constraint trên bảng PhieuMuon nếu dòng này phát sinh lỗi tương tự.
            // Nếu PhieuMuon cũng dùng "DaMuon" thì đổi "DangMuon" -> "DaMuon"
            pm.TrangThai = "DangMuon";

            data.PhieuMuon.Add(pm);
            data.SaveChanges(); // Lưu phiếu mượn để lấy MaPhieuMuon tự tăng

            // ====== BƯỚC 2: Lưu Chi Tiết & Cập Nhật Cuốn Sách =====
            foreach (var item in gioHang.lst)
            {
                // Lấy ra các cuốn sách có sẵn tương ứng với mã sách hiện tại
                var availableCopies = data.CuonSach
                    .Where(cs => cs.MaSach == item.iMaSach && cs.TrangThai == "SanSang")
                    .Take(item.iSoLuong)
                    .ToList();

                foreach (var cuonSach in availableCopies)
                {
                    // Tạo chi tiết phiếu mượn
                    ChiTietPhieuMuon ct = new ChiTietPhieuMuon();
                    ct.MaPhieuMuon = pm.MaPhieuMuon;
                    ct.MaCuonSach = cuonSach.MaCuonSach;
                    ct.NgayTraThucTe = null;
                    ct.TinhTrangKhiTra = null;

                    data.ChiTietPhieuMuon.Add(ct);

                    // FIX LỖI TẠI ĐÂY: Đổi "DangMuon" thành "DaMuon" cho đúng CHECK constraint
                    cuonSach.TrangThai = "DaMuon";
                }
            }

            // Lưu tất cả thay đổi của ChiTietPhieuMuon và CuonSach
            try
            {
                data.SaveChanges();
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

            // Làm sạch giỏ hàng sau khi đăng ký thành công
            Session["GioHang"] = null;

            TempData["SuccessMessage"] = "Đăng ký mượn sách thành công! Vui lòng nhận sách tại Quầy thủ thư trong vòng 24 giờ.";

            return RedirectToAction("Index");
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
    }
}
