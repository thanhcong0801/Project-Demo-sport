using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;
using System.Linq;
using System.Threading.Tasks;

namespace TheBallStores.Controllers
{
    public class DoanhThuController : Controller
    {
        private readonly TheballStoreContext _context;

        public DoanhThuController(TheballStoreContext context)
        {
            _context = context;
        }

        // Trang báo cáo chính
        public async Task<IActionResult> Index()
        {
            // BẢO MẬT: Chỉ Admin được vào
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy 10 đơn hàng gần nhất
            var lastOrders = await _context.DonHangs
                .OrderByDescending(d => d.NgayDat)
                .Take(10)
                .ToListAsync();

            ViewBag.LastOrders = lastOrders;

            // === LOGIC TÍNH TOÁN CHÍNH XÁC (ĐÃ SỬA LỖI NULL) ===

            // 1. Tổng Doanh thu (Chỉ tính đơn đã Hoàn thành)
            // Sửa lỗi: Chuyển đổi d.TongTien ?? 0 để đảm bảo không cộng null
            var completedOrders = _context.DonHangs.Where(d => d.TrangThai == "Hoàn thành");

            // Cách fix: Select ra TongTien trước, xử lý null, rồi mới Sum
            ViewBag.TotalRevenue = await completedOrders
                .Select(d => d.TongTien ?? 0)
                .SumAsync();

            // 2. Tổng Số Đơn Hàng (Trừ đơn Đã hủy)
            ViewBag.TotalOrders = await _context.DonHangs
                .CountAsync(d => d.TrangThai != "Đã hủy");

            // 3. Đơn mới cần xử lý (COD mới đặt hoặc Online chờ thanh toán)
            ViewBag.NewOrders = await _context.DonHangs
                .CountAsync(d => d.TrangThai == "Mới đặt (COD)" || d.TrangThai == "Chờ thanh toán");

            // ===========================================

            return View();
        }
    }
}