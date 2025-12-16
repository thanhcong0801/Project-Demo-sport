using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace TheBallStores.Models
{
    [Table("LoaiSanPhams")]
    public partial class LoaiSanPham
    {
        public LoaiSanPham()
        {
            SanPhams = new HashSet<SanPham>();
        }

        [Key]
        [Display(Name = "Mã Loại")]
        public int MaLoai { get; set; }

        [Required(ErrorMessage = "Tên loại sản phẩm là bắt buộc.")]
        [StringLength(100)]
        [Display(Name = "Tên Loại")]
        public string TenLoai { get; set; } = null!;

        [StringLength(50)]
        [Display(Name = "Nhóm Loại")]
        public string? NhomLoai { get; set; }

        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}