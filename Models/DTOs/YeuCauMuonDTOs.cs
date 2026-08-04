using System;
using System.Collections.Generic;

namespace DoAn_LTWeb.Models.DTOs
{
    public class YeuCauMuonDto
    {
        public int MaYeuCau { get; set; }
        public int MaDocGia { get; set; }
        public string TenDocGia { get; set; }
        public string NgayGui { get; set; }
        public string TrangThai { get; set; }
        public int SoLuongSach { get; set; }
    }

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
}
