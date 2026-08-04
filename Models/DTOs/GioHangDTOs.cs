using System;
using System.Collections.Generic;
using System.Linq;

namespace DoAn_LTWeb.Models.DTOs
{
    public class CartItem
    {
        public int iMaSach { get; set; }
        public string sTenSach { get; set; }
        public string sAnhBia { get; set; }
        public int iSoLuong { get; set; }

        public CartItem()
        {
        }

        public CartItem(Sach sach)
        {
            iMaSach = sach.MaSach;
            sTenSach = sach.TenSach;
            sAnhBia = sach.AnhBia;
            iSoLuong = 1;
        }
    }

    public class GioHang
    {
        public List<CartItem> lst;

        public GioHang()
        {
            lst = new List<CartItem>();
        }

        public GioHang(List<CartItem> ds)
        {
            lst = ds;
        }

        // Số loại sách
        public int SoMatHang()
        {
            return lst.Count;
        }

        // Tổng số lượng sách
        public int TongSLHang()
        {
            return lst.Sum(x => x.iSoLuong);
        }

        // Thêm sách
        public void Them(Sach sach)
        {
            CartItem item = lst.FirstOrDefault(x => x.iMaSach == sach.MaSach);

            if (item == null)
            {
                lst.Add(new CartItem(sach));
            }
            else
            {
                item.iSoLuong++;
            }
        }

        // Xóa
        public void Xoa(int maSach)
        {
            CartItem item = lst.FirstOrDefault(x => x.iMaSach == maSach);

            if (item != null)
                lst.Remove(item);
        }

        // Giảm số lượng
        public void Giam(int maSach)
        {
            CartItem item = lst.FirstOrDefault(x => x.iMaSach == maSach);

            if (item != null)
            {
                item.iSoLuong--;

                if (item.iSoLuong == 0)
                    lst.Remove(item);
            }
        }

        // Xóa toàn bộ
        public void XoaTatCa()
        {
            lst.Clear();
        }

        // Cập nhật số lượng
        public void CapNhat(int maSach, int soLuongMoi)
        {
            CartItem item = lst.FirstOrDefault(x => x.iMaSach == maSach);
            if (item != null)
            {
                item.iSoLuong = soLuongMoi;
            }
        }
    }
}
