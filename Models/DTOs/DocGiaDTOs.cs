using System;
using System.Collections.Generic;

namespace DoAn_LTWeb.Models.DTOs
{
    public class TaiKhoanDocGiaDto
    {
        public NguoiDung NguoiDung { get; set; }
        public List<YeuCauMuonDocGiaDto> DanhSachYeuCau { get; set; }
        public List<PhieuMuonDocGiaDto> DanhSachPhieuMuon { get; set; }
        public List<PhieuPhatDocGiaDto> DanhSachPhieuPhat { get; set; }
        public string ActiveTab { get; set; }

        public TaiKhoanDocGiaDto()
        {
            DanhSachYeuCau = new List<YeuCauMuonDocGiaDto>();
            DanhSachPhieuMuon = new List<PhieuMuonDocGiaDto>();
            DanhSachPhieuPhat = new List<PhieuPhatDocGiaDto>();
            ActiveTab = "profile";
        }
    }

    public class CapNhatThongTinInput
    {
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
    }

    public class DoiMatKhauInput
    {
        public string MatKhauCu { get; set; }
        public string MatKhauMoi { get; set; }
        public string XacNhanMatKhau { get; set; }
    }

    public class YeuCauMuonDocGiaDto
    {
        public int MaYeuCau { get; set; }
        public DateTime? NgayYeuCau { get; set; }
        public string TrangThai { get; set; }
        public List<ChiTietYeuCauItemDto> DanhSachSach { get; set; }

        public YeuCauMuonDocGiaDto()
        {
            DanhSachSach = new List<ChiTietYeuCauItemDto>();
        }
    }

    public class ChiTietYeuCauItemDto
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; }
        public string AnhBia { get; set; }
        public string TenTacGia { get; set; }
        public int SoLuong { get; set; }
    }

    public class PhieuMuonDocGiaDto
    {
        public int MaPhieuMuon { get; set; }
        public DateTime? NgayMuon { get; set; }
        public DateTime? NgayHenTra { get; set; }
        public string TrangThai { get; set; }
        public string TenThuThu { get; set; }
        public List<ChiTietPhieuMuonItemDto> DanhSachCuonSach { get; set; }

        public PhieuMuonDocGiaDto()
        {
            DanhSachCuonSach = new List<ChiTietPhieuMuonItemDto>();
        }
    }

    public class ChiTietPhieuMuonItemDto
    {
        public int MaChiTiet { get; set; }
        public int MaCuonSach { get; set; }
        public string TenSach { get; set; }
        public string AnhBia { get; set; }
        public string TenTacGia { get; set; }
        public DateTime? NgayTraThucTe { get; set; }
        public string TinhTrangKhiTra { get; set; }
    }

    public class PhieuPhatDocGiaDto
    {
        public int MaPhieuPhat { get; set; }
        public int MaPhieuMuon { get; set; }
        public string TenSach { get; set; }
        public string AnhBia { get; set; }
        public int MaCuonSach { get; set; }
        public decimal SoTienPhat { get; set; }
        public string LyDoPhat { get; set; }
        public DateTime? NgayLap { get; set; }
        public string TrangThaiThanhToan { get; set; }
    }

    public class MenuTheLoaiCap2Dto
    {
        public int MaTheLoai { get; set; }
        public string TenTheLoai { get; set; }
        public List<TacGiaMenuDto> DanhSachTacGia { get; set; }
        public List<SachMenuDto> DanhSachSachNoiBat { get; set; }

        public MenuTheLoaiCap2Dto()
        {
            DanhSachTacGia = new List<TacGiaMenuDto>();
            DanhSachSachNoiBat = new List<SachMenuDto>();
        }
    }

    public class TacGiaMenuDto
    {
        public int MaTacGia { get; set; }
        public string TenTacGia { get; set; }
        public int SoLuongSach { get; set; }
    }

    public class SachMenuDto
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; }
        public string AnhBia { get; set; }
    }

    public class DocGiaTraCuuItemDto
    {
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public string TrangThaiThe { get; set; }
        public DateTime? NgayTao { get; set; }
        public int SoLuotMuon { get; set; }
        public int DangMuonCount { get; set; }
        public decimal TongPhatChuaNop { get; set; }
    }

    public class DocGiaChiTietModalDto
    {
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public string TrangThaiThe { get; set; }
        public DateTime? NgayTao { get; set; }
        public List<PhieuMuonDocGiaDto> DanhSachPhieuMuon { get; set; }
        public List<PhieuPhatDocGiaDto> DanhSachPhieuPhat { get; set; }

        public DocGiaChiTietModalDto()
        {
            DanhSachPhieuMuon = new List<PhieuMuonDocGiaDto>();
            DanhSachPhieuPhat = new List<PhieuPhatDocGiaDto>();
        }
    }
}
