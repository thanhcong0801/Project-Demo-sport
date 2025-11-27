using Microsoft.AspNetCore.Mvc;

namespace TheBallStores.Controllers
{
    public class AdminController : Controller
    {
        // Trang Dashboard chính
        public IActionResult Index()
        {
            // Kiểm tra bảo mật: Phải đăng nhập VÀ là Admin mới được vào
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}