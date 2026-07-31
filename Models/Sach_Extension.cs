using System;
using System.Linq;

namespace DoAn_LTWeb.Models
{
    public partial class Sach
    {
        /// <summary>
        /// Tính toán động số lượng sách đang có sẵn trên kệ (chưa bị mượn)
        /// </summary>
        public int SoLuongChuaMuon
        {
            get
            {
                if (this.CuonSach == null)
                {
                    return 0;
                }
                // Đếm các cuốn sách vật lý có trạng thái là "SanSang" hoặc "Sẵn sàng"
                return this.CuonSach.Count(cs => cs.TrangThai != null && 
                                                 (cs.TrangThai.Trim().Equals("SanSang", StringComparison.OrdinalIgnoreCase) ||
                                                  cs.TrangThai.Trim().Equals("Sẵn sàng", StringComparison.OrdinalIgnoreCase)));
            }
        }

        /// <summary>
        /// Tổng số lượng sách được đếm động từ các cuốn sách vật lý tương ứng
        /// </summary>
        public Nullable<int> TongSoLuong
        {
            get
            {
                return this.CuonSach == null ? 0 : (int?)this.CuonSach.Count;
            }
        }
    }
}
