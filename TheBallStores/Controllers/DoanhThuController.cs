using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;
using System.Linq;

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
            // Giả định DonHang có MaKhNavigation và TenNguoiNhan
            var lastOrders = await _context.DonHangs
                .OrderByDescending(d => d.NgayDat)
                .Take(10)
                .ToListAsync();

            ViewBag.LastOrders = lastOrders;

            // Thống kê đơn giản
            ViewBag.TotalRevenue = _context.DonHangs.Where(d => d.TrangThai == "Hoàn thành").Sum(d => d.TongTien) ?? 0;
            ViewBag.TotalOrders = _context.DonHangs.Count();
            ViewBag.NewOrders = _context.DonHangs.Count(d => d.TrangThai == "Mới đặt");

            return View();
        }

        // (Thêm các Action khác như QuanLyDonHang, ThongKe...)
    }
}