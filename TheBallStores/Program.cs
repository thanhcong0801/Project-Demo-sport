using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;
using TheBallStores.Helpers;
using TheBallStores.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.StaticFiles; // Thêm namespace này

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- FIX LỖI DATA PROTECTION TRÊN RENDER ---
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "keys");
if (!Directory.Exists(keysFolder)) Directory.CreateDirectory(keysFolder);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("TheBallStores");
// ------------------------------------------

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
// PHẦN QUAN TRỌNG: TỰ ĐỘNG KHỞI TẠO VÀ SỬA LỖI DATABASE
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TheballStoreContext>();

        // --- LOGIC TỰ ĐỘNG SỬA LỖI ---
        try
        {
            if (!context.Database.CanConnect() || !context.KhachHangs.Any())
            {
                context.Database.Migrate();
            }
        }
        catch
        {
            Console.WriteLine("--> Phát hiện lỗi Database Schema. Đang Reset lại Database...");
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }
        // -----------------------------

        // Gọi hàm Initialize để thêm dữ liệu mẫu (Admin, Sản phẩm...)
        DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi nghiêm trọng khi khởi tạo Database (Seeding DB).");
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

// --- SỬA Ở ĐÂY: CẤU HÌNH ĐỂ SERVER HIỂU FILE .AVIF ---
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
// -----------------------------------------------------

app.UseRouting();

app.UseSession(); // Kích hoạt Session

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Store}/{action=Index}/{id?}");

app.Run();