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
                // 1. Đảm bảo Database đã được tạo
                context.Database.Migrate();

                // 2. TẠO TÀI KHOẢN ADMIN (Luôn kiểm tra trước tiên)
                if (!context.KhachHangs.Any(k => k.Email == "admin@gmail.com"))
                {
                    var adminUser = new KhachHang
                    {
                        HoTen = "Admin Quản Trị",
                        Email = "admin@gmail.com",
                        MatKhau = "123456", // Pass demo
                        DiaChi = "Hồ Chí Minh",
                        DienThoai = "0909000111",
                        VaiTro = "Admin"
                        // Đã xóa NgaySinh, GioiTinh, HieuLuc vì Model không có
                    };
                    context.KhachHangs.Add(adminUser);
                    context.SaveChanges(); // Lưu ngay để có thể đăng nhập được liền
                }

                // 3. TẠO TÀI KHOẢN KHÁCH DEMO (Nếu chưa có)
                if (!context.KhachHangs.Any(k => k.Email == "khach@gmail.com"))
                {
                    var customerUser = new KhachHang
                    {
                        HoTen = "Khách Hàng Demo",
                        Email = "khach@gmail.com",
                        MatKhau = "123456",
                        DiaChi = "Hà Nội",
                        DienThoai = "0909000222",
                        VaiTro = "Customer"
                    };
                    context.KhachHangs.Add(customerUser);
                    context.SaveChanges();
                }

                // 4. KIỂM TRA DỮ LIỆU SẢN PHẨM
                // Nếu đã có sản phẩm rồi thì thôi, không seed thêm để tránh trùng lặp
                if (context.SanPhams.Any())
                {
                    return;
                }

                // --- NẾU CHƯA CÓ SẢN PHẨM THÌ TẠO MỚI (SEED DATA) ---

                // A. Tạo Danh Mục
                var loaiSanPhams = new LoaiSanPham[]
                {
                    new LoaiSanPham { TenLoai = "Giày Bóng Đá" },
                    new LoaiSanPham { TenLoai = "Áo Bóng Đá" },
                    new LoaiSanPham { TenLoai = "Phụ Kiện" }
                };
                context.LoaiSanPhams.AddRange(loaiSanPhams);
                context.SaveChanges();

                // B. Lấy ID danh mục vừa tạo
                var giayBongDa = context.LoaiSanPhams.FirstOrDefault(l => l.TenLoai == "Giày Bóng Đá");
                var aoBongDa = context.LoaiSanPhams.FirstOrDefault(l => l.TenLoai == "Áo Bóng Đá");
                int maGiay = giayBongDa?.MaLoai ?? 1;
                int maAo = aoBongDa?.MaLoai ?? 2;

                // C. Tạo Sản Phẩm
                var sanPhams = new SanPham[]
                {
                    new SanPham
                    {
                        TenSp = "Nike Mercurial Superfly 9",
                        GiaBan = 2500000,
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

                // D. Tạo Voucher
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