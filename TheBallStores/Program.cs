using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;
using TheBallStores.Helpers;
using TheBallStores.Data; // Bắt buộc phải có dòng này để dùng DbInitializer

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình Session (Giữ nguyên cấu hình cũ của bạn)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cấu hình Database SQLite
// Lưu ý: Đảm bảo chuỗi kết nối trong appsettings.json là "Data Source=theballstore.db"
builder.Services.AddDbContext<TheballStoreContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("TheballStoreContext")));

var app = builder.Build();

// =========================================================================
// PHẦN QUAN TRỌNG: TỰ ĐỘNG KHỞI TẠO DATABASE VÀ NẠP DỮ LIỆU
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Gọi hàm Initialize để tạo bảng và thêm dữ liệu nếu chưa có
        DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi xảy ra khi khởi tạo dữ liệu (Seeding DB).");
    }
}
// =========================================================================

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Kích hoạt Session

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Store}/{action=Index}/{id?}");

app.Run();