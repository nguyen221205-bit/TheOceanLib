using System;
using System.Collections.Generic;

namespace DoAn_LTWeb.Models.DTOs
{
    public class TraSachDetailDto
    {
        public int MaDocGia { get; set; }
        public string TenDocGia { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string NgayMuon { get; set; }
        public string NgayHenTra { get; set; }
        public List<TraSachBookDto> Books { get; set; }
    }

    public class TraSachBookDto
    {
        public int MaChiTiet { get; set; }
        public int MaCuonSach { get; set; }
        public string TenSach { get; set; }
        public string AnhBia { get; set; }
        public string ViTriKe { get; set; }
        public int GiaSach { get; set; }
        public string NgayTraThucTe { get; set; }
        public string TinhTrangKhiTra { get; set; }
    }

    public class TraSachSubmitInput
    {
        public int MaPhieuMuon { get; set; }
        public List<TraSachItemInput> Items { get; set; }
        public decimal TongTienPhat { get; set; }
        public string LyDoPhat { get; set; }
        public string TrangThaiThanhToan { get; set; } // "Chưa thanh toán" / "Đã thanh toán"
    }

    public class TraSachItemInput
    {
        public int MaChiTiet { get; set; }
        public int MaCuonSach { get; set; }
        public bool TraSach { get; set; }
        public string TinhTrang { get; set; } // "Bình thường", "Hỏng", "Mất"
        public decimal TienPhatRieng { get; set; }
        public string LyDoPhatRieng { get; set; }
    }
}
