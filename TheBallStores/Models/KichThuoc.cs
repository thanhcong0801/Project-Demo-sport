using System;
using System.Collections.Generic;

namespace TheBallStores.Models
{
    public partial class KichThuoc
    {
        public KichThuoc()
        {
            SanPhamChiTiets = new HashSet<SanPhamChiTiet>();
        }

        public int MaSize { get; set; }
        public string TenSize { get; set; } = null!;

        public virtual ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}
