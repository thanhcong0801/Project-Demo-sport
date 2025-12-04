using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using TheBallStores.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký DbContext
builder.Services.AddDbContext<TheballStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TheballStoreContext")));

// 2. Đăng ký dịch vụ MVC
builder.Services.AddControllersWithViews();

// === [ĐÃ CẤU HÌNH] ĐĂNG KÝ DỊCH VỤ SESSION (Sẽ được gọi bởi builder) ===
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session (30 phút)
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// =========================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cấu hình hỗ trợ file .avif (Code cũ của bạn)
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

// === [ĐÃ KÍCH HOẠT] MIDDLEWARE SESSION (Phải đặt trước UseRouting) ===
app.UseSession();
// ======================================================================

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();