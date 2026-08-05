using System;
using System.Collections.Generic;

namespace DoAn_LTWeb.Models.DTOs
{
    public class SachApiInputDto
    {
        public string TenSach { get; set; }
        public int MaTheLoai { get; set; }
        public int MaTacGia { get; set; }
        public int MaNXB { get; set; }
        public int? NamXuatBan { get; set; }
        public string MoTa { get; set; }
        public string AnhBia { get; set; }
    }

    public class SachApiResultDto
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; }
        public int MaTheLoai { get; set; }
        public string TenTheLoai { get; set; }
        public int MaTacGia { get; set; }
        public string TenTacGia { get; set; }
        public int MaNXB { get; set; }
        public string TenNXB { get; set; }
        public int? NamXuatBan { get; set; }
        public string MoTa { get; set; }
        public string AnhBia { get; set; }
        public int TongSoBan { get; set; }
        public int SoLuongCoSan { get; set; }
    }
}
