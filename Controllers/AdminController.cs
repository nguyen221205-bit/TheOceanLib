using System;
using System.Collections.Generic;
using System.Linq;
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

        // GET: Admin/QuanLySach
        public ActionResult QuanLySach()
        {
            ViewBag.ActiveMenu = "QuanLySach";
            ViewBag.Title = "Quản lý đầu sách & kho bản sách";
            return View();
        }

        // GET: Admin/QuanLyTheLoai
        public ActionResult QuanLyTheLoai()
        {
            ViewBag.ActiveMenu = "QuanLyTheLoai";
            ViewBag.Title = "Quản lý danh mục Thể loại";
            return View();
        }

        // GET: Admin/QuanLyTacGia
        public ActionResult QuanLyTacGia()
        {
            ViewBag.ActiveMenu = "QuanLyTacGia";
            ViewBag.Title = "Quản lý danh mục Tác giả";
            return View();
        }

        // GET: Admin/QuanLyNXB
        public ActionResult QuanLyNXB()
        {
            ViewBag.ActiveMenu = "QuanLyNXB";
            ViewBag.Title = "Quản lý Nhà xuất bản";
            return View();
        }

        // GET: Admin/QuanLyTaiKhoan
        public ActionResult QuanLyTaiKhoan()
        {
            ViewBag.ActiveMenu = "QuanLyTaiKhoan";
            ViewBag.Title = "Quản lý tài khoản & phân quyền";
            return View();
        }

        // GET: Admin/BaoCaoThongKe
        public ActionResult BaoCaoThongKe()
        {
            ViewBag.ActiveMenu = "BaoCaoThongKe";
            ViewBag.Title = "Báo cáo thống kê & doanh thu";
            return View();
        }
    }
}
