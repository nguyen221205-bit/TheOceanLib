using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DoAn_LTWeb.Models;
using DoAn_LTWeb.Models.DTOs;

namespace DoAn_LTWeb.Controllers
{
    /// <summary>
    /// RESTful Web API Controller dành cho Quản Lý Sách (Full CRUD - GET, POST, PUT, DELETE)
    /// Hỗ trợ kiểm thử trực tiếp qua Postman, cURL hoặc Trang Console Test API trên Admin.
    /// </summary>
    public class SachApiController : Controller
    {
        private QuanLyThuVienEntities data = new QuanLyThuVienEntities();

        // 1. GET: /api/SachApi (Lấy danh sách tất cả các cuốn sách)
        [HttpGet]
        public ActionResult Index(string search, int? maTheLoai, int? page, int? pageSize)
        {
            try
            {
                var query = data.Sach
                    .Include(s => s.TheLoai)
                    .Include(s => s.TacGia)
                    .Include(s => s.NhaXuatBan)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string kw = search.Trim().ToLower();
                    query = query.Where(s => s.TenSach.ToLower().Contains(kw) || s.TacGia.TenTacGia.ToLower().Contains(kw));
                }

                if (maTheLoai.HasValue && maTheLoai.Value > 0)
                {
                    query = query.Where(s => s.MaTheLoai == maTheLoai.Value);
                }

                int totalItem = query.Count();
                int pSize = (pageSize.HasValue && pageSize.Value > 0) ? pageSize.Value : 20;
                int pIndex = (page.HasValue && page.Value > 0) ? page.Value : 1;

                var list = query
                    .OrderByDescending(s => s.MaSach)
                    .Skip((pIndex - 1) * pSize)
                    .Take(pSize)
                    .ToList()
                    .Select(s => new SachApiResultDto
                    {
                        MaSach = s.MaSach,
                        TenSach = s.TenSach,
                        MaTheLoai = s.MaTheLoai,
                        TenTheLoai = s.TheLoai != null ? s.TheLoai.TenTheLoai : "N/A",
                        MaTacGia = s.MaTacGia,
                        TenTacGia = s.TacGia != null ? s.TacGia.TenTacGia : "N/A",
                        MaNXB = s.MaNXB,
                        TenNXB = s.NhaXuatBan != null ? s.NhaXuatBan.TenNXB : "N/A",
                        NamXuatBan = s.NamXuatBan,
                        MoTa = s.MoTa,
                        AnhBia = s.AnhBia,
                        TongSoBan = data.CuonSach.Count(cs => cs.MaSach == s.MaSach),
                        SoLuongCoSan = data.CuonSach.Count(cs => cs.MaSach == s.MaSach && cs.TrangThai == "Có sẵn")
                    })
                    .ToList();

                return Json(new
                {
                    statusCode = 200,
                    status = "OK",
                    totalItems = totalItem,
                    currentPage = pIndex,
                    pageSize = pSize,
                    data = list
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { statusCode = 500, status = "Internal Server Error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // 2. GET: /api/SachApi/GetById/5 (Lấy chi tiết 1 cuốn sách theo ID)
        [HttpGet]
        public ActionResult GetById(int id)
        {
            try
            {
                var s = data.Sach
                    .Include(x => x.TheLoai)
                    .Include(x => x.TacGia)
                    .Include(x => x.NhaXuatBan)
                    .FirstOrDefault(x => x.MaSach == id);

                if (s == null)
                {
                    return Json(new { statusCode = 404, status = "Not Found", message = $"Không tìm thấy cuốn sách có Mã #{id} trong hệ thống CSDL!" }, JsonRequestBehavior.AllowGet);
                }

                var dto = new SachApiResultDto
                {
                    MaSach = s.MaSach,
                    TenSach = s.TenSach,
                    MaTheLoai = s.MaTheLoai,
                    TenTheLoai = s.TheLoai != null ? s.TheLoai.TenTheLoai : "N/A",
                    MaTacGia = s.MaTacGia,
                    TenTacGia = s.TacGia != null ? s.TacGia.TenTacGia : "N/A",
                    MaNXB = s.MaNXB,
                    TenNXB = s.NhaXuatBan != null ? s.NhaXuatBan.TenNXB : "N/A",
                    NamXuatBan = s.NamXuatBan,
                    MoTa = s.MoTa,
                    AnhBia = s.AnhBia,
                    TongSoBan = data.CuonSach.Count(cs => cs.MaSach == s.MaSach),
                    SoLuongCoSan = data.CuonSach.Count(cs => cs.MaSach == s.MaSach && cs.TrangThai == "Có sẵn")
                };

                return Json(new { statusCode = 200, status = "OK", data = dto }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { statusCode = 500, status = "Internal Server Error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // 3. POST: /api/SachApi/Create (Thêm mới 1 cuốn sách vào CSDL)
        [HttpPost]
        public ActionResult Create(SachApiInputDto input)
        {
            try
            {
                if (input == null || string.IsNullOrWhiteSpace(input.TenSach))
                {
                    return Json(new { statusCode = 400, status = "Bad Request", message = "Tên sách không được để trống!" });
                }

                if (input.MaTheLoai <= 0 || input.MaTacGia <= 0 || input.MaNXB <= 0)
                {
                    return Json(new { statusCode = 400, status = "Bad Request", message = "Mã Thể loại, Tác giả và Nhà xuất bản phải hợp lệ!" });
                }

                Sach s = new Sach
                {
                    TenSach = input.TenSach.Trim(),
                    MaTheLoai = input.MaTheLoai,
                    MaTacGia = input.MaTacGia,
                    MaNXB = input.MaNXB,
                    NamXuatBan = input.NamXuatBan,
                    MoTa = input.MoTa,
                    AnhBia = string.IsNullOrWhiteSpace(input.AnhBia) ? "default-book.jpg" : input.AnhBia.Trim()
                };

                data.Sach.Add(s);
                data.SaveChanges();

                // Load lại thông tin quan hệ
                var createdBook = data.Sach
                    .Include(x => x.TheLoai)
                    .Include(x => x.TacGia)
                    .Include(x => x.NhaXuatBan)
                    .FirstOrDefault(x => x.MaSach == s.MaSach);

                var dto = new SachApiResultDto
                {
                    MaSach = createdBook.MaSach,
                    TenSach = createdBook.TenSach,
                    MaTheLoai = createdBook.MaTheLoai,
                    TenTheLoai = createdBook.TheLoai != null ? createdBook.TheLoai.TenTheLoai : "N/A",
                    MaTacGia = createdBook.MaTacGia,
                    TenTacGia = createdBook.TacGia != null ? createdBook.TacGia.TenTacGia : "N/A",
                    MaNXB = createdBook.MaNXB,
                    TenNXB = createdBook.NhaXuatBan != null ? createdBook.NhaXuatBan.TenNXB : "N/A",
                    NamXuatBan = createdBook.NamXuatBan,
                    MoTa = createdBook.MoTa,
                    AnhBia = createdBook.AnhBia,
                    TongSoBan = 0,
                    SoLuongCoSan = 0
                };

                return Json(new { statusCode = 201, status = "Created", message = $"Thêm mới thành công cuốn sách '{s.TenSach}' với Mã #{s.MaSach}!", data = dto });
            }
            catch (Exception ex)
            {
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                return Json(new { statusCode = 500, status = "Internal Server Error", message = "Lỗi CSDL: " + inner.Message });
            }
        }

        // 4. PUT: /api/SachApi/Update/5 (Cập nhật thông tin sách)
        [HttpPost] // Hoặc PUT
        public ActionResult Update(int id, SachApiInputDto input)
        {
            try
            {
                var s = data.Sach.FirstOrDefault(x => x.MaSach == id);
                if (s == null)
                {
                    return Json(new { statusCode = 404, status = "Not Found", message = $"Không tìm thấy cuốn sách Mã #{id} để cập nhật!" });
                }

                if (input != null)
                {
                    if (!string.IsNullOrWhiteSpace(input.TenSach)) s.TenSach = input.TenSach.Trim();
                    if (input.MaTheLoai > 0) s.MaTheLoai = input.MaTheLoai;
                    if (input.MaTacGia > 0) s.MaTacGia = input.MaTacGia;
                    if (input.MaNXB > 0) s.MaNXB = input.MaNXB;
                    if (input.NamXuatBan.HasValue) s.NamXuatBan = input.NamXuatBan.Value;
                    if (input.MoTa != null) s.MoTa = input.MoTa;
                    if (!string.IsNullOrWhiteSpace(input.AnhBia)) s.AnhBia = input.AnhBia.Trim();
                }

                data.SaveChanges();

                var updatedBook = data.Sach
                    .Include(x => x.TheLoai)
                    .Include(x => x.TacGia)
                    .Include(x => x.NhaXuatBan)
                    .FirstOrDefault(x => x.MaSach == id);

                var dto = new SachApiResultDto
                {
                    MaSach = updatedBook.MaSach,
                    TenSach = updatedBook.TenSach,
                    MaTheLoai = updatedBook.MaTheLoai,
                    TenTheLoai = updatedBook.TheLoai != null ? updatedBook.TheLoai.TenTheLoai : "N/A",
                    MaTacGia = updatedBook.MaTacGia,
                    TenTacGia = updatedBook.TacGia != null ? updatedBook.TacGia.TenTacGia : "N/A",
                    MaNXB = updatedBook.MaNXB,
                    TenNXB = updatedBook.NhaXuatBan != null ? updatedBook.NhaXuatBan.TenNXB : "N/A",
                    NamXuatBan = updatedBook.NamXuatBan,
                    MoTa = updatedBook.MoTa,
                    AnhBia = updatedBook.AnhBia,
                    TongSoBan = data.CuonSach.Count(cs => cs.MaSach == id),
                    SoLuongCoSan = data.CuonSach.Count(cs => cs.MaSach == id && cs.TrangThai == "Có sẵn")
                };

                return Json(new { statusCode = 200, status = "OK", message = $"Đã cập nhật thông tin cuốn sách #{id} thành công!", data = dto });
            }
            catch (Exception ex)
            {
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                return Json(new { statusCode = 500, status = "Internal Server Error", message = "Lỗi CSDL: " + inner.Message });
            }
        }

        // 5. DELETE: /api/SachApi/Delete/5 (Xóa cuốn sách)
        [HttpPost] // Hoặc DELETE
        public ActionResult Delete(int id)
        {
            try
            {
                var s = data.Sach.FirstOrDefault(x => x.MaSach == id);
                if (s == null)
                {
                    return Json(new { statusCode = 404, status = "Not Found", message = $"Không tìm thấy cuốn sách Mã #{id} để xóa!" });
                }

                // Kiểm tra ràng buộc sách đang được mượn hoặc có chi tiết yêu cầu mượn
                int countCopies = data.CuonSach.Count(cs => cs.MaSach == id);
                if (countCopies > 0)
                {
                    return Json(new { statusCode = 400, status = "Bad Request", message = $"Không thể xóa cuốn sách #{id} vì kho hiện đang có {countCopies} bản sách vật lý liên quan! Vui lòng xóa các bản sách trước." });
                }

                string deletedTitle = s.TenSach;
                data.Sach.Remove(s);
                data.SaveChanges();

                return Json(new { statusCode = 200, status = "OK", message = $"Đã xóa cuốn sách '{deletedTitle}' (Mã #{id}) thành công khỏi CSDL!" });
            }
            catch (Exception ex)
            {
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                return Json(new { statusCode = 500, status = "Internal Server Error", message = "Không thể xóa sách do vướng ràng buộc khóa ngoại: " + inner.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                data.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
