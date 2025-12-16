using System.Collections.Generic;

namespace TheBallStores.Models
{
    public class GioHangViewModel
    {
        // Danh sách các sản phẩm trong giỏ (Sử dụng class GioHangItem)
        public List<GioHangItem> Items { get; set; } = new List<GioHangItem>();

        public decimal TongTienHang { get; set; }

        public decimal SoTienGiam { get; set; }

        public decimal TongThanhToan { get; set; }

        // Cho phép null (?) để tránh lỗi biên dịch
        public string? MaVoucher { get; set; }

        public string? ThongBao { get; set; }
    }
}