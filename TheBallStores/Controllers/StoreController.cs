using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Models; // Sửa namespace nếu cần

namespace TheBallStores.Controllers
{
    public class StoreController : Controller
    {
        private readonly TheballStoreContext _context;

        public StoreController(TheballStoreContext context)
        {
            _context = context;
        }

        // Trang danh sách sản phẩm (Full tính năng: Lọc, Tìm kiếm)
        public async Task<IActionResult> Index(int? maLoai, string searchString)
        {
            // Kiểm tra đăng nhập: Nếu chưa đăng nhập thì đá về trang Login
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var sanPhams = _context.SanPhams.Include(s => s.MaLoaiNavigation).AsQueryable();

            if (maLoai.HasValue)
            {
                sanPhams = sanPhams.Where(s => s.MaLoai == maLoai.Value);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                sanPhams = sanPhams.Where(s => s.TenSp.Contains(searchString));
            }

            var cacLoai = await _context.LoaiSanPhams.ToListAsync();
            ViewBag.CacLoai = cacLoai;
            ViewBag.MaLoaiHienTai = maLoai;
            ViewBag.SearchString = searchString;

            return View(await sanPhams.ToListAsync());
        }

        // Trang chi tiết sản phẩm
        public async Task<IActionResult> Details(int? id)
        {
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null || _context.SanPhams == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.MaLoaiNavigation)
                .Include(s => s.SanPhamChiTiets)
                    .ThenInclude(kho => kho.MaSizeNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }
    }
}