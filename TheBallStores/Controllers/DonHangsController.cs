using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;

namespace TheBallStores.Controllers
{
    public class DonHangsController : Controller
    {
        private readonly TheballStoreContext _context;

        public DonHangsController(TheballStoreContext context)
        {
            _context = context;
        }

        // GET: DonHangs/Index (ADMIN)
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction(nameof(LichSuGiaoDich));
            }

            var donHangs = await _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(donHangs);
        }

        // GET: DonHangs/LichSuGiaoDich (CUSTOMER)
        public async Task<IActionResult> LichSuGiaoDich()
        {
            // FIX LỖI: Kiểm tra session an toàn
            var maKh = HttpContext.Session.GetInt32("MaKh");
            if (maKh == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var donHangs = await _context.DonHangs
                .Where(d => d.MaKh == maKh.Value)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(donHangs);
        }

        // ... Các action Details, CapNhatTrangThai, HuyDonHang giữ nguyên logic cũ ...
        // (Nếu cần code full đoạn này bạn báo mình nhé, nhưng quan trọng nhất là Index và LichSuGiaoDich đã fix ở trên)

        // GET: DonHangs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Check login
            if (HttpContext.Session.GetString("HoTen") == null) return RedirectToAction("Login", "Account");

            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.MaChiTietNavigation).ThenInclude(spct => spct.MaSpNavigation)
                .Include(d => d.MaKhNavigation)
                .FirstOrDefaultAsync(m => m.MaDonHang == id);

            if (donHang == null) return NotFound();

            // Check quyền xem
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                var maKh = HttpContext.Session.GetInt32("MaKh");
                if (donHang.MaKh != maKh) return RedirectToAction(nameof(LichSuGiaoDich));
            }

            return View(donHang);
        }

        // POST: CapNhatTrangThai (Admin)
        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(int maDonHang, string trangThaiMoi)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin") return RedirectToAction("Index", "Home");

            var donHang = await _context.DonHangs.FindAsync(maDonHang);
            if (donHang != null)
            {
                donHang.TrangThai = trangThaiMoi;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = maDonHang });
        }

        // POST: HuyDonHang (Khach)
        public async Task<IActionResult> HuyDonHang(int id)
        {
            var maKh = HttpContext.Session.GetInt32("MaKh");
            if (maKh == null) return RedirectToAction("Login", "Account");

            var donHang = await _context.DonHangs.Include(d => d.ChiTietDonHangs).FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (donHang != null && donHang.MaKh == maKh)
            {
                if (donHang.TrangThai == "Mới đặt" || donHang.TrangThai == "Mới đặt (COD)" || donHang.TrangThai == "Chờ thanh toán")
                {
                    donHang.TrangThai = "Đã hủy";

                    // Hoàn kho
                    foreach (var ct in donHang.ChiTietDonHangs)
                    {
                        var spct = await _context.SanPhamChiTiets.FindAsync(ct.MaChiTiet);
                        if (spct != null) spct.SoLuongTon = (spct.SoLuongTon ?? 0) + (ct.SoLuong ?? 0);
                    }
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Hủy đơn hàng thành công.";
                }
            }
            return RedirectToAction(nameof(LichSuGiaoDich));
        }
    }
}