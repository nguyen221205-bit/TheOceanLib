using System;
using System.Collections.Generic;

namespace DoAn_LTWeb.Models.DTOs
{
    public class PhieuPhatQuanLyDto
    {
        public int MaPhieuPhat { get; set; }
        public int MaChiTiet { get; set; }
        public int MaPhieuMuon { get; set; }
        public int MaDocGia { get; set; }
        public string TenDocGia { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public decimal SoTienPhat { get; set; }
        public string LyDo { get; set; }
        public string NgayLap { get; set; }
        public string TrangThaiThanhToan { get; set; } // "ChuaThanhToan" / "DaThanhToan"
        public string TenSach { get; set; }
    }

    public class PhieuPhatDetailDto
    {
        public int MaPhieuPhat { get; set; }
        public int MaPhieuMuon { get; set; }
        public int MaDocGia { get; set; }
        public string TenDocGia { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public decimal SoTienPhat { get; set; }
        public string LyDo { get; set; }
        public string NgayLap { get; set; }
        public string TrangThaiThanhToan { get; set; } // "ChuaThanhToan" / "DaThanhToan"
        public int MaCuonSach { get; set; }
        public string TenSach { get; set; }
        public string TinhTrangKhiTra { get; set; }
    }

    public class ThanhToanPhieuPhatInput
    {
        public int MaPhieuPhat { get; set; }
        public string PhuongThucThanhToan { get; set; } // "TienMat" / "ChuyenKhoan"
    }
}
