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
                // 1. Luôn chạy lệnh này để đảm bảo bảng được tạo (nếu lỡ tay xóa db)
                context.Database.Migrate();

                // 2. CHỈ GIỮ LẠI LOGIC TẠO ADMIN (Để cứu hộ khi cần thiết)
                if (!context.KhachHangs.Any(k => k.Email == "admin@gmail.com"))
                {
                    var adminUser = new KhachHang
                    {
                        HoTen = "Admin Quản Trị",
                        Email = "admin@gmail.com",
                        MatKhau = "123456",
                        DiaChi = "Hồ Chí Minh",
                        DienThoai = "0909000111",
                        VaiTro = "Admin"
                    };
                    context.KhachHangs.Add(adminUser);
                    context.SaveChanges();
                }
            }
        }
    }
}