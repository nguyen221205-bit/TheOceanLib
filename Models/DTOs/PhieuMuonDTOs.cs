using System;

namespace DoAn_LTWeb.Models.DTOs
{
    public class PhieuMuonQuanLyDto
    {
        public int MaPhieuMuon { get; set; }
        public int MaDocGia { get; set; }
        public string TenDocGia { get; set; }
        public string NgayMuon { get; set; }
        public string NgayHenTra { get; set; }
        public string TrangThai { get; set; }
        public int SoLuongSach { get; set; }
        public string DanhSachTenSach { get; set; }
    }

    public class PhieuMuonDetailQuanLyDto
    {
        public int MaChiTiet { get; set; }
        public int MaCuonSach { get; set; }
        public string TenSach { get; set; }
        public string ViTriKe { get; set; }
        public string NgayTraThucTe { get; set; }
        public string TinhTrangKhiTra { get; set; }
    }

    public class TraSachInput
    {
        public int MaChiTiet { get; set; }
        public int MaCuonSach { get; set; }
        public string TinhTrang { get; set; } // "Bình thường", "Hỏng", "Mất"
    }
}
