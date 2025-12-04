using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;

namespace TheBallStores.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly TheballStoreContext _context;

        public TaiKhoanController(TheballStoreContext context)
        {
            _context = context;
        }

        // Trang danh sách Khách hàng/Admin
        public async Task<IActionResult> Index()
        {
            // Bảo mật: Chỉ Admin được vào
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            var khachHangs = await _context.KhachHangs.ToListAsync();
            return View(khachHangs);
        }

        // *Bạn có thể bổ sung các hàm Edit, Delete, Details cho Tài khoản ở đây*
    }
}