using System.Collections.Generic;
using System.Linq;

namespace DoAn_LTWeb.Models
{
    public class Sach
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; }
        public string AnhBia { get; set; }
        public int MaTacGia { get; set; }
        public int TongSoLuong { get; set; }
        public string ISBN { get; set; }
        public int NamXuatBan { get; set; }
        public string MoTa { get; set; }
    }

    public class TacGia
    {
        public int MaTacGia { get; set; }
        public string TenTacGia { get; set; }
    }

    public class QuanLyThuVienEntities1
    {
        public List<Sach> Saches { get; set; }
        public List<TacGia> TacGias { get; set; }

        public QuanLyThuVienEntities1()
        {
            // Seed mock data
            TacGias = new List<TacGia>
            {
                new TacGia { MaTacGia = 1, TenTacGia = "Nguyễn Nhật Ánh" },
                new TacGia { MaTacGia = 2, TenTacGia = "Tô Hoài" },
                new TacGia { MaTacGia = 3, TenTacGia = "Dale Carnegie" }
            };

            Saches = new List<Sach>
            {
                new Sach
                {
                    MaSach = 1,
                    TenSach = "Mắt Biếc",
                    AnhBia = "mat_biec.jpg",
                    MaTacGia = 1,
                    TongSoLuong = 10,
                    ISBN = "978-604-1-1823-4",
                    NamXuatBan = 2019,
                    MoTa = "Một trong những tác phẩm tiêu biểu của nhà văn Nguyễn Nhật Ánh, kể về tình yêu thời học trò ngây thơ."
                },
                new Sach
                {
                    MaSach = 2,
                    TenSach = "Dế Mèn Phiêu Lưu Ký",
                    AnhBia = "de_men.jpg",
                    MaTacGia = 2,
                    TongSoLuong = 5,
                    ISBN = "978-604-2-1234-5",
                    NamXuatBan = 1941,
                    MoTa = "Tác phẩm văn học thiếu nhi kinh điển của nhà văn Tô Hoài, viết về cuộc phiêu lưu của chú Dế Mèn."
                },
                new Sach
                {
                    MaSach = 3,
                    TenSach = "Đắc Nhân Tâm",
                    AnhBia = "dac_nhan_tam.jpg",
                    MaTacGia = 3,
                    TongSoLuong = 8,
                    ISBN = "978-604-3-5678-9",
                    NamXuatBan = 1936,
                    MoTa = "Cuốn sách tự lực (self-help) bán chạy nhất mọi thời đại, giúp cải thiện kỹ năng giao tiếp và đối nhân xử thế."
                }
            };
        }
    }
}
