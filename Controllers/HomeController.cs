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

        public ActionResult HienThiDSSP()
        {
            List<Sach> dssp = data.Sach.Include("TacGia").Include("CuonSach").ToList();
            return View(dssp);
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



    }
}
