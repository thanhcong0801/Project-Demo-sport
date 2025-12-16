using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheBallStores.Models
{
    // Ánh xạ đến tên bảng số ít (theo DDL chuẩn của bạn)
    [Table("HinhAnhSanPham")]
    public class HinhAnhSanPham
    {
        [Key]
        public int MaAnh { get; set; }

        [Required]
        public int MaSp { get; set; }

        [Required]
        [StringLength(255)]
        public string DuongDanAnh { get; set; } = string.Empty; // Khởi tạo để tránh lỗi null C#

        [ForeignKey("MaSp")]
        public virtual SanPham? MaSpNavigation { get; set; }
    }
}