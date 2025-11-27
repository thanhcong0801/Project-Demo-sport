using System;
using System.Collections.Generic;

namespace TheBallStores.Models
{
    public partial class LoaiSanPham
    {
        public LoaiSanPham()
        {
            SanPhams = new HashSet<SanPham>();
        }

        public int MaLoai { get; set; }
        public string TenLoai { get; set; } = null!;
        public string? NhomLoai { get; set; }

        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}
