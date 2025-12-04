namespace TheBallStores.Models
{
    public class GioHangItem
    {
        public int MaSp { get; set; }

        // Khởi tạo giá trị mặc định để tránh lỗi null
        public string TenSp { get; set; } = string.Empty;
        public string AnhDaiDien { get; set; } = string.Empty;

        public decimal DonGia { get; set; }
        public int MaSize { get; set; }

        // Khởi tạo giá trị mặc định
        public string TenSize { get; set; } = string.Empty;

        public int SoLuong { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}