using Microsoft.AspNetCore.Mvc;
using TheBallStores.Models; // Sửa namespace nếu cần

namespace TheBallStores.Controllers
{
    public class AdminController : Controller
    {
        // Khai báo Context (để tương tác với DB)
        private readonly TheballStoreContext _context;

        public AdminController(TheballStoreContext context)
        {
            _context = context;
        }

        // BẢO MẬT: Kiểm tra Admin cho toàn bộ Controller
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("VaiTro") == "Admin";
        }

        // Trang Dashboard chính
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                // Nếu không phải Admin, chuyển hướng về trang chủ
                return RedirectToAction("Index", "Home");
            }

            // (Tùy chọn) Lấy dữ liệu thống kê cơ bản
            // Cần sửa lại cột TrangThai nếu cần
            ViewBag.TotalProducts = _context.SanPhams.Count();
            ViewBag.TotalCustomers = _context.KhachHangs.Count();
            // Đảm bảo cột TrangThai là NVARCHAR(50) và có dữ liệu "Mới đặt"
            ViewBag.NewOrders = _context.DonHangs.Count(d => d.TrangThai == "Mới đặt");

            return View();
        }

        // Action quản lý sản phẩm (Ví dụ)
        public IActionResult QuanLySanPham()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            // Logic chuyển hướng đến trang CRUD Sản phẩm
            return RedirectToAction("Index", "SanPhams");
        }

        // (Thêm các Action khác như QuanLyTaiKhoan, QuanLyDonHang...)
    }
}