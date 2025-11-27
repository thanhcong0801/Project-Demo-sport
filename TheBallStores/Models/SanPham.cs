using System;
using System.Collections.Generic;

namespace TheBallStores.Models
{
    public partial class SanPham
    {
        public SanPham()
        {
            SanPhamChiTiets = new HashSet<SanPhamChiTiet>();
        }

        public int MaSp { get; set; }
        public string TenSp { get; set; } = null!;
        public decimal GiaBan { get; set; }
        public string? AnhDaiDien { get; set; }
        public string? MoTa { get; set; }
        public int? MaLoai { get; set; }

        public virtual LoaiSanPham? MaLoaiNavigation { get; set; }
        public virtual ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}
