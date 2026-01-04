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
            // 1. Bảo mật: Chỉ Admin được vào
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (string.IsNullOrEmpty(vaiTro) || vaiTro != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Lấy 10 đơn hàng mới nhất để hiển thị
            var lastOrders = await _context.DonHangs
                .OrderByDescending(d => d.NgayDat)
                .Take(10)
                .ToListAsync();

            ViewBag.LastOrders = lastOrders;

            // === FIX LỖI SQLITE SUM DECIMAL (QUAN TRỌNG) ===
            // Thay vì dùng .SumAsync() (gây lỗi trên SQLite), ta dùng .ToListAsync() trước
            // Lấy hết các cột TongTien về RAM rồi mới cộng

            var paidOrders = await _context.DonHangs
                .Where(d => d.TrangThai == "Đã thanh toán (VNPay)" || d.TrangThai == "Hoàn thành")
                .Select(d => d.TongTien) // Chỉ lấy cột tiền cho nhẹ
                .ToListAsync(); // <--- Tải về RAM tại đây

            // Bây giờ cộng trên RAM (C# xử lý) -> Không bao giờ lỗi
            decimal totalRevenue = paidOrders.Sum(t => t ?? 0);

            ViewBag.TotalRevenue = totalRevenue;
            // ===============================================

            // 3. Tổng số đơn hàng (Trừ đơn hủy)
            var totalOrders = await _context.DonHangs
                .CountAsync(d => d.TrangThai != "Đã hủy" && d.TrangThai != "Hủy (Lỗi thanh toán)");

            ViewBag.TotalOrders = totalOrders;

            // 4. Đơn mới cần xử lý
            var newOrderStatuses = new[] { "Mới đặt (COD)", "Chờ thanh toán", "Mới đặt" };
            var newOrdersCount = await _context.DonHangs
                .CountAsync(d => newOrderStatuses.Contains(d.TrangThai));

            ViewBag.NewOrders = newOrdersCount;

            return View();
        }
    }
}