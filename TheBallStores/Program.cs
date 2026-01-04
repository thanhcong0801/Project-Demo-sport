using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;
using TheBallStores.Helpers;
using TheBallStores.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.StaticFiles; // Để xử lý ảnh .avif

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- FIX LỖI SESSION & DATA PROTECTION TRÊN RENDER ---
// Lưu khóa bảo mật vào thư mục 'keys' để không bị mất khi restart app
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "keys");
if (!Directory.Exists(keysFolder)) Directory.CreateDirectory(keysFolder);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("TheBallStores");
// -----------------------------------------------------

// Cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cấu hình Database SQLite
builder.Services.AddDbContext<TheballStoreContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("TheballStoreContext")));

var app = builder.Build();

// =========================================================================
// TỰ ĐỘNG KHỞI TẠO & SỬA LỖI DATABASE
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TheballStoreContext>();

        // Tự động tạo bảng nếu chưa có (Fix lỗi no such table)
        try
        {
            context.Database.Migrate();
        }
        catch
        {
            // Nếu lỗi nặng quá thì xóa đi tạo lại (Chỉ dùng khi cần thiết)
            // context.Database.EnsureDeleted();
            // context.Database.Migrate();
        }

        // Nạp dữ liệu mẫu (Admin,...)
        DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi khởi tạo Database.");
    }
}
// =========================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// --- CẤU HÌNH ĐỂ HIỂN THỊ ẢNH .AVIF ---
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
// --------------------------------------

app.UseRouting();

app.UseSession(); // Kích hoạt Session

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Store}/{action=Index}/{id?}");

app.Run();