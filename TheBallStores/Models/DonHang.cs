using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheBallStores.Models
{
    [Table("DonHangs")]
    public partial class DonHang
    {
        public DonHang()
        {
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
        }

        [Key]
        public int MaDonHang { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? NgayDat { get; set; }

        [Column("MaKH")]
        public int? MaKh { get; set; }

        [Column(TypeName = "decimal(18, 0)")]
        public decimal? TongTien { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }

        [StringLength(100)]
        public string? TenNguoiNhan { get; set; }

        [StringLength(200)]
        public string? DiaChiGiaoHang { get; set; }

        [StringLength(15)]
        [Unicode(false)]
        public string? SoDienThoaiNguoiNhan { get; set; }

        // FIX: BỔ SUNG Thuộc tính Voucher
        [StringLength(50)]
        public string? MaVoucher { get; set; }

        [Column(TypeName = "decimal(18, 0)")]
        public decimal? SoTienGiam { get; set; }

        [ForeignKey("MaKh")]
        [InverseProperty("DonHangs")]
        public virtual KhachHang? MaKhNavigation { get; set; }

        [InverseProperty("MaDonHangNavigation")]
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}