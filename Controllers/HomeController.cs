using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAn_LTWeb.Models;
namespace DoAn_LTWeb.Controllers
{
    public class HomeController : Controller
    {
        QuanLyThuVienEntities data = new QuanLyThuVienEntities();
        public ActionResult Index()
        {
            List<Sach> dssp = data.Sach.Include("TacGia").Include("TheLoai").Include("CuonSach").Take(4).ToList();
            return View(dssp);
        }

        public ActionResult HienThiDSSP(int? maTheLoai)
        {
            List<Sach> dssp;
            if (maTheLoai.HasValue)
            {
                dssp = data.Sach.Include("TacGia").Include("CuonSach")
                                 .Where(s => s.MaTheLoai == maTheLoai.Value).ToList();
                var theLoai = data.TheLoai.FirstOrDefault(tl => tl.MaTheLoai == maTheLoai.Value);
                ViewBag.TenTheLoai = theLoai != null ? theLoai.TenTheLoai : "";
            }
            else
            {
                dssp = data.Sach.Include("TacGia").Include("CuonSach").ToList();
            }
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

            // TODO: Ở đây bạn có thể thêm code lưu vào bảng PhieuMuon và ChiTietPhieuMuon trong CSDL của bạn:
            // PhieuMuon pm = new PhieuMuon();
            // pm.MaDocGia = 1; // ID của độc giả mặc định hoặc độc giả đang đăng nhập
            // pm.NgayMuon = DateTime.Parse(ngayMuon);
            // pm.NgayHenTra = DateTime.Parse(ngayTra);
            // pm.TrangThai = "ChoDuyet";
            // data.PhieuMuon.Add(pm);
            // data.SaveChanges();
            // ... lưu tiếp ChiTietPhieuMuon ...

            // Làm sạch giỏ hàng sau khi đăng ký thành công
            Session["GioHang"] = null;

            // Đặt thông báo TempData để hiển thị thông báo thành công ở trang chủ
            TempData["SuccessMessage"] = "Đăng ký mượn sách thành công! Vui lòng nhận sách tại Quầy thủ thư trong vòng 24 giờ.";

            return RedirectToAction("Index");
        }
    }
}
