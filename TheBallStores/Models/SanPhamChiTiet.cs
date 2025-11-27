using System;
using System.Collections.Generic;

namespace TheBallStores.Models
{
    public partial class SanPhamChiTiet
    {
        public SanPhamChiTiet()
        {
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
        }

        public int MaChiTiet { get; set; }
        public int? MaSp { get; set; }
        public int? MaSize { get; set; }
        public int? SoLuongTon { get; set; }

        public virtual KichThuoc? MaSizeNavigation { get; set; }
        public virtual SanPham? MaSpNavigation { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}
