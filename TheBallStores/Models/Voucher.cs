using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheBallStores.Models
{
    [Table("Voucher")]
    public class Voucher
    {
        [Key]
        [StringLength(50, ErrorMessage = "Mã voucher tối đa 50 ký tự")]
        [Display(Name = "Mã Voucher")]
        public string MaVoucher { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập tên chương trình")]
        [StringLength(255)]
        [Display(Name = "Tên Chương Trình")]
        public string TenChuongTrinh { get; set; } = null!;

        [Required]
        [Display(Name = "Loại Giảm Giá")] // 1: % (Phần trăm), 2: VNĐ (Tiền mặt)
        public int LoaiGiamGia { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá trị giảm")]
        [Column(TypeName = "decimal(18, 0)")]
        [Display(Name = "Giá Trị Giảm")]
        [Range(1, double.MaxValue, ErrorMessage = "Giá trị giảm phải lớn hơn 0")]
        public decimal GiaTriGiam { get; set; }

        // CẢI THIỆN: Thêm giới hạn giảm tối đa (Quan trọng cho giảm theo %)
        [Column(TypeName = "decimal(18, 0)")]
        [Display(Name = "Giảm Tối Đa")]
        public decimal? GiamToiDa { get; set; }

        // CẢI THIỆN: Thêm điều kiện đơn hàng tối thiểu
        [Column(TypeName = "decimal(18, 0)")]
        [Display(Name = "Đơn Tối Thiểu")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn tối thiểu không được âm")]
        public decimal DonToiThieu { get; set; } = 0;

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [Column(TypeName = "datetime")]
        [Display(Name = "Ngày Bắt Đầu")]
        public DateTime NgayBatDau { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [Column(TypeName = "datetime")]
        [Display(Name = "Ngày Kết Thúc")]
        public DateTime NgayKetThuc { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Display(Name = "Số Lượng Còn Lại")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int SoLuongConLai { get; set; }

        // CẢI THIỆN: Trạng thái kích hoạt để bật/tắt nhanh
        [Display(Name = "Kích Hoạt")]
        public bool KichHoat { get; set; } = true;

        // CẢI THIỆN: Ngày tạo để quản lý lịch sử
        [Column(TypeName = "datetime")]
        [Display(Name = "Ngày Tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Helper: Kiểm tra nhanh tính hợp lệ
        public bool IsValid()
        {
            var now = DateTime.Now;
            return KichHoat &&
                   SoLuongConLai > 0 &&
                   now >= NgayBatDau &&
                   now <= NgayKetThuc;
        }
    }
}