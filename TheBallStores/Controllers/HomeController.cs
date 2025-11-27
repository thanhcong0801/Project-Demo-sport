using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TheBallStores.Models; // ⚠️ Kiểm tra namespace này khớp với project của bạn

namespace TheBallStores.Controllers
{
    public class HomeController : Controller
    {
        private readonly TheballStoreContext _context;

        public HomeController(TheballStoreContext context)
        {
            _context = context;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index(int? maLoai, string searchString) // <-- Thêm searchString vào đây
        {
            // 1. Khởi tạo query lấy sản phẩm (chưa thực thi)
            var sanPhams = _context.SanPhams
                                   .Include(s => s.MaLoaiNavigation)
                                   .AsQueryable();

            // 2. Lọc theo danh mục (nếu có)
            if (maLoai.HasValue)
            {
                sanPhams = sanPhams.Where(s => s.MaLoai == maLoai.Value);
            }

            // 3. Tìm kiếm theo tên (nếu có nhập) - Code Mới
            if (!string.IsNullOrEmpty(searchString))
            {
                sanPhams = sanPhams.Where(s => s.TenSp.Contains(searchString));
            }

            // 4. Lấy danh sách loại để hiện Menu bên trái
            var cacLoai = await _context.LoaiSanPhams.ToListAsync();
            ViewBag.CacLoai = cacLoai;
            ViewBag.MaLoaiHienTai = maLoai;

            // Lưu lại từ khóa tìm kiếm để hiện lại trên ô input
            ViewBag.SearchString = searchString;

            // 5. Lấy 4 sản phẩm ngẫu nhiên NẾU không tìm kiếm/lọc (để hiển thị Carousel ở trang chủ)
            // Hoặc trả về danh sách kết quả tìm kiếm
            if (!maLoai.HasValue && string.IsNullOrEmpty(searchString))
            {
                // Nếu vào trang chủ bình thường -> Lấy 4 sản phẩm ngẫu nhiên cho đẹp
                return View(await sanPhams.OrderBy(r => Guid.NewGuid()).Take(4).ToListAsync());
            }

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