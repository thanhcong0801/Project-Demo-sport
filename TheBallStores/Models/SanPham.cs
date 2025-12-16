using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace TheBallStores.Models
{
    [Table("SanPhams")]
    public partial class SanPham
    {
        public SanPham()
        {
            HinhAnhSanPhams = new HashSet<HinhAnhSanPham>();
            SanPhamChiTiets = new HashSet<SanPhamChiTiet>();
        }

        [Key]
        [Column("MaSP")]
        public int MaSp { get; set; }

        [Required]
        [StringLength(200)]
        [Column("TenSP")]
        [Display(Name = "Tên Sản Phẩm")]
        public string TenSp { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18, 0)")]
        [Display(Name = "Giá Bán")]
        public decimal GiaBan { get; set; }

        [StringLength(200)]
        [Display(Name = "Ảnh Đại Diện")]
        public string? AnhDaiDien { get; set; }

        [Display(Name = "Mô Tả")]
        public string? MoTa { get; set; }

        public int? MaLoai { get; set; }

        [ForeignKey("MaLoai")]
        [InverseProperty("SanPhams")]
        [Display(Name = "Loại Sản Phẩm")]
        public virtual LoaiSanPham? MaLoaiNavigation { get; set; }

        // FIX: BỔ SUNG Navigation Property cho Ảnh phụ
        [InverseProperty("MaSpNavigation")]
        public virtual ICollection<HinhAnhSanPham> HinhAnhSanPhams { get; set; }

        [InverseProperty("MaSpNavigation")]
        public virtual ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}