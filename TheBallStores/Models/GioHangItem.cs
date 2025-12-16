namespace TheBallStores.Models
{
    public class GioHangItem
    {
        public int MaSp { get; set; }
        public string TenSp { get; set; } = string.Empty;
        public string AnhDaiDien { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public int MaSize { get; set; }
        public string TenSize { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}
