using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Cần thiết cho FindAsync hoặc FirstOrDefault
using TheBallStores.Models;
using System.Security.Cryptography;
using System.Text;

namespace TheBallStores.Controllers
{
    public class AccountController : Controller
    {
        private readonly TheballStoreContext _context;

        public AccountController(TheballStoreContext context)
        {
            _context = context;
        }

        // GET: Đăng ký
        public IActionResult Register()
        {
            return View();
        }

        // POST: Đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra null cho Email trước khi query
                if (string.IsNullOrEmpty(khachHang.Email))
                {
                    ViewBag.Error = "Email không được để trống!";
                    return View();
                }

                // Kiểm tra email trùng (Xử lý an toàn null)
                var checkEmail = _context.KhachHangs.FirstOrDefault(k => k.Email == khachHang.Email);
                if (checkEmail != null)
                {
                    ViewBag.Error = "Email này đã được sử dụng!";
                    return View();
                }

                // Gán vai trò mặc định là Customer
                khachHang.VaiTro = "Customer";

                // Đảm bảo các trường không null nếu cần thiết
                khachHang.HoTen ??= "";
                khachHang.MatKhau ??= "";

                _context.Add(khachHang);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View();
        }

        // GET: Đăng nhập
        public IActionResult Login()
        {
            return View();
        }

        // POST: Đăng nhập
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập Email và Mật khẩu!";
                return View();
            }

            // Tìm user trong DB
            // Lưu ý: SQLite phân biệt hoa thường, nên ta dùng ToLower() hoặc so sánh chính xác tùy nhu cầu
            // Ở đây dùng so sánh trực tiếp để tận dụng Index
            var user = _context.KhachHangs.FirstOrDefault(u => u.Email == email && u.MatKhau == password);

            if (user != null)
            {
                // 1. Lưu Session (Sử dụng toán tử null-coalescing ?? để tránh lỗi nếu dữ liệu null)
                HttpContext.Session.SetString("Email", user.Email ?? "");
                HttpContext.Session.SetString("HoTen", user.HoTen ?? "Khách hàng");

                // Chuẩn hóa vai trò về chữ thường hoặc chữ hoa đầu để so sánh dễ dàng hơn
                string vaiTro = user.VaiTro ?? "Customer";
                HttpContext.Session.SetString("VaiTro", vaiTro);

                // === LƯU MÃ KHÁCH HÀNG VÀO SESSION ===
                HttpContext.Session.SetInt32("MaKh", user.MaKh);
                // ==============================================

                // 2. Phân quyền chuyển hướng
                // So sánh không phân biệt hoa thường để tránh lỗi "admin" vs "Admin"
                if (string.Equals(vaiTro, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Admin"); // Chuyển sang trang Admin
                }
                else
                {
                    return RedirectToAction("Index", "Store"); // Chuyển về trang chủ mua hàng
                }
            }

            ViewBag.Error = "Sai thông tin đăng nhập hoặc mật khẩu!";
            return View();
        }

        // Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}