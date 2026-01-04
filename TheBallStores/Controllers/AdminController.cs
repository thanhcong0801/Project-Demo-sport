using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;

namespace TheBallStores.Controllers
{
    public class AdminController : Controller
    {
        private readonly TheballStoreContext _context;

        public AdminController(TheballStoreContext context)
        {
            _context = context;
        }

        // BẢO MẬT: Kiểm tra Admin
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("VaiTro") == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            // Sử dụng CountAsync để tối ưu
            ViewBag.TotalProducts = await _context.SanPhams.CountAsync();

            ViewBag.TotalCustomers = await _context.KhachHangs.CountAsync();

            // Đếm đơn mới (Khớp với chuỗi trong DB SQLite đã sửa)
            ViewBag.NewOrders = await _context.DonHangs
                .CountAsync(d => d.TrangThai == "Mới đặt" || d.TrangThai == "Mới đặt (COD)");

            return View();
        }

        // --- CÁC CHỨC NĂNG ĐIỀU HƯỚNG QUẢN LÝ ---
        // Giúp giữ các chức năng liên kết từ Dashboard

        public IActionResult QuanLySanPham()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");
            return RedirectToAction("Index", "SanPhams");
        }

        public IActionResult QuanLyDonHang()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");
            return RedirectToAction("Index", "DonHangs");
        }

        public IActionResult QuanLyKhachHang()
        {
            // Chuyển hướng sang TaiKhoanController (hoặc KhachHangsController tùy tên bạn đặt)
            if (!IsAdmin()) return RedirectToAction("Index", "Home");
            return RedirectToAction("Index", "TaiKhoan");
        }

        public IActionResult BaoCaoDoanhThu()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");
            return RedirectToAction("Index", "DoanhThu");
        }
    }
}