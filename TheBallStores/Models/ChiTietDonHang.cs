using System;
using System.Collections.Generic;

namespace TheBallStores.Models
{
    public partial class ChiTietDonHang
    {
        public int MaCt { get; set; }
        public int? MaDonHang { get; set; }
        public int? MaChiTiet { get; set; }
        public int? SoLuong { get; set; }
        public decimal? DonGia { get; set; }

        public virtual SanPhamChiTiet? MaChiTietNavigation { get; set; }
        public virtual DonHang? MaDonHangNavigation { get; set; }
    }
}
