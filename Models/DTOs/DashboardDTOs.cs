using System;

namespace DoAn_LTWeb.Models.DTOs
{
    public class RecentPhieuMuonDto
    {
        public int MaPhieuMuon { get; set; }
        public int MaDocGia { get; set; }
        public string TenDocGia { get; set; }
        public string NgayMuon { get; set; }
        public string NgayHenTra { get; set; }
        public string TenSach { get; set; }
        public string TrangThai { get; set; }
    }

    public class TheLoaiThongKeDto
    {
        public string TenTheLoai { get; set; }
        public int SoLuong { get; set; }
    }
}
