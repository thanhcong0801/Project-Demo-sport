using System;
using System.Collections.Generic;

namespace TheBallStores.Models
{
    public partial class KhachHang
    {
        public KhachHang()
        {
            DonHangs = new HashSet<DonHang>();
        }

        public int MaKh { get; set; }
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? MatKhau { get; set; }
        public string? DiaChi { get; set; }
        public string? DienThoai { get; set; }
        public string? VaiTro { get; set; }

        public virtual ICollection<DonHang> DonHangs { get; set; }
    }
}
