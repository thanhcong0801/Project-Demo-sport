using Microsoft.AspNetCore.Mvc;
using TheBallStores.Models; // Sửa thành namespace của bạn
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
                // Kiểm tra email trùng
                var checkEmail = _context.KhachHangs.FirstOrDefault(k => k.Email == khachHang.Email);
                if (checkEmail != null)
                {
                    ViewBag.Error = "Email này đã được sử dụng!";
                    return View();
                }

                // Gán vai trò mặc định là Customer
                khachHang.VaiTro = "Customer";
                // Lưu ý: Trong thực tế nên mã hóa mật khẩu ở đây

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
            // Tìm user trong DB (So sánh plain text cho đơn giản, thực tế cần hash)
            var user = _context.KhachHangs.FirstOrDefault(u => u.Email == email && u.MatKhau == password);

            if (user != null)
            {
                // 1. Lưu Session
                HttpContext.Session.SetString("Email", user.Email ?? ""); // Nếu Email null thì lưu chuỗi rỗng
                HttpContext.Session.SetString("HoTen", user.HoTen ?? "Khách hàng"); // Nếu HoTen null thì lưu là "Khách hàng"
                HttpContext.Session.SetString("VaiTro", user.VaiTro ?? "Customer"); // Nếu VaiTro null thì mặc định là "Customer"

                // 2. Phân quyền chuyển hướng
                if (user.VaiTro == "Admin")
                {
                    return RedirectToAction("Index", "Admin"); // Chuyển sang trang Admin
                }
                else
                {
                    return RedirectToAction("Index", "Home"); // Chuyển về trang chủ
                }
            }

            ViewBag.Error = "Sai thông tin đăng nhập!";
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