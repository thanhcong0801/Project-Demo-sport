using System;
using System.Collections.Generic;

namespace TheBallStores.Models
{
    public partial class DonHang
    {
        public DonHang()
        {
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
        }

        public int MaDonHang { get; set; }
        public DateTime? NgayDat { get; set; }
        public int? MaKh { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThai { get; set; }
        public string? TenNguoiNhan { get; set; }
        public string? DiaChiGiaoHang { get; set; }
        public string? SoDienThoaiNguoiNhan { get; set; }

        public virtual KhachHang? MaKhNavigation { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}
