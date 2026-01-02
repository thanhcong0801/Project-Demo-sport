using TheBallStores.Models;
using Microsoft.EntityFrameworkCore;

namespace TheBallStores.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new TheballStoreContext(
                serviceProvider.GetRequiredService<DbContextOptions<TheballStoreContext>>()))
            {
                // Tự động migrate nếu chưa có DB (quan trọng cho Render)
                context.Database.Migrate();

                // Kiểm tra xem đã có dữ liệu chưa
                if (context.SanPhams.Any())
                {
                    return;   // DB đã có dữ liệu thì không làm gì cả
                }

                // --- 1. SEED DANH MỤC (Loại Sản Phẩm) ---
                var loaiSanPhams = new LoaiSanPham[]
                {
                    // Đã xóa thuộc tính 'MoTa' vì Model của bạn không có
                    new LoaiSanPham { TenLoai = "Giày Bóng Đá" },
                    new LoaiSanPham { TenLoai = "Áo Bóng Đá" },
                    new LoaiSanPham { TenLoai = "Phụ Kiện" }
                };
                context.LoaiSanPhams.AddRange(loaiSanPhams);
                context.SaveChanges();

                // --- 2. SEED SẢN PHẨM ---
                // Lấy MaLoai thực tế sau khi insert
                var giayBongDa = context.LoaiSanPhams.FirstOrDefault(l => l.TenLoai == "Giày Bóng Đá");
                var aoBongDa = context.LoaiSanPhams.FirstOrDefault(l => l.TenLoai == "Áo Bóng Đá");

                // Dùng toán tử null-coalescing (??) để đảm bảo không bị lỗi null
                int maGiay = giayBongDa?.MaLoai ?? 1;
                int maAo = aoBongDa?.MaLoai ?? 2;

                var sanPhams = new SanPham[]
                {
                    new SanPham
                    {
                        TenSp = "Nike Mercurial Superfly 9",
                        GiaBan = 2500000, 
                        // Đã xóa 'MoTa', 'SoLuongTon', 'NgayTao' vì Model không có
                        AnhDaiDien = "nike_mercurial_9.jpg",
                        MaLoai = maGiay
                    },
                    new SanPham
                    {
                        TenSp = "Adidas X Crazyfast",
                        GiaBan = 2300000,
                        AnhDaiDien = "adidas_x_crazyfast.jpg",
                        MaLoai = maGiay
                    },
                    new SanPham
                    {
                        TenSp = "Áo Manchester United 2024",
                        GiaBan = 180000,
                        AnhDaiDien = "mu_home_2024.jpg",
                        MaLoai = maAo
                    }
                };
                context.SanPhams.AddRange(sanPhams);

                // --- 3. SEED VOUCHER ---
                var vouchers = new Voucher[]
                {
                    new Voucher
                    {
                        MaVoucher = "GIAM10",
                        TenChuongTrinh = "Chào mừng thành viên mới",
                        LoaiGiamGia = 1,
                        GiaTriGiam = 10,
                        DonToiThieu = 0,
                        GiamToiDa = 50000,
                        NgayBatDau = DateTime.Now.AddDays(-1),
                        NgayKetThuc = DateTime.Now.AddMonths(1),
                        SoLuongConLai = 100,
                        KichHoat = true,
                        NgayTao = DateTime.Now
                    }
                };
                context.Vouchers.AddRange(vouchers);

                context.SaveChanges();
            }
        }
    }
}