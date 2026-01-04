using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TheBallStores.Models;

namespace TheBallStores.Controllers
{
    public class HomeController : Controller
    {
        private readonly TheballStoreContext _context;

        public HomeController(TheballStoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? maLoai, string searchString)
        {
            var sanPhams = _context.SanPhams
                                   .Include(s => s.MaLoaiNavigation)
                                   .AsQueryable();

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

            // --- FIX LỖI RANDOM TRÊN SQLITE ---
            // Thay vì dùng OrderBy(Guid.NewGuid()) gây lỗi, ta dùng Skip ngẫu nhiên
            if (!maLoai.HasValue && string.IsNullOrEmpty(searchString))
            {
                int total = await sanPhams.CountAsync();
                int take = 4; // Số lượng sản phẩm muốn hiển thị ngẫu nhiên

                if (total > take)
                {
                    int skip = new Random().Next(0, total - take);
                    return View(await sanPhams.Skip(skip).Take(take).ToListAsync());
                }
                return View(await sanPhams.ToListAsync());
            }
            // ----------------------------------

            return View(await sanPhams.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}