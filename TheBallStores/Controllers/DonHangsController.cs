using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;
using System.Linq;
using System.Threading.Tasks;

namespace TheBallStores.Controllers
{
    public class DonHangsController : Controller
    {
        private readonly TheballStoreContext _context;

        public DonHangsController(TheballStoreContext context)
        {
            _context = context;
        }

        // GET: DonHangs/Index (CHỈ DÀNH CHO ADMIN - Quản lý tất cả đơn hàng)
        public async Task<IActionResult> Index()
        {
            // BẢO MẬT: Nếu không phải Admin, chuyển sang Lịch sử giao dịch của Khách hàng
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction(nameof(LichSuGiaoDich));
            }

            // Admin thấy tất cả đơn hàng
            var donHangs = await _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(donHangs);
        }

        // GET: DonHangs/LichSuGiaoDich (DÀNH CHO KHÁCH HÀNG - Chỉ thấy đơn của mình)
        public async Task<IActionResult> LichSuGiaoDich()
        {
            // Yêu cầu phải đăng nhập
            if (HttpContext.Session.GetInt32("MaKh") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Sửa lỗi: Kiểm tra xem Session có null không trước khi lấy Value
            var maKhSession = HttpContext.Session.GetInt32("MaKh");
            if (maKhSession == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int maKh = maKhSession.Value;

            // Khách hàng chỉ thấy đơn của chính mình
            var donHangs = await _context.DonHangs
                .Where(d => d.MaKh == maKh)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(donHangs);
        }

        // GET: DonHangs/Details/5 (Xem chi tiết đơn hàng)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // KIỂM TRA BẢO MẬT: Nếu chưa đăng nhập, chuyển hướng
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy đơn hàng (Include tất cả các bảng liên quan)
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaChiTietNavigation) // Nối bảng SanPhamChiTiet
                        .ThenInclude(spct => spct.MaSpNavigation) // Nối bảng SanPham
                .Include(d => d.MaKhNavigation)
                .FirstOrDefaultAsync(m => m.MaDonHang == id);

            if (donHang == null) return NotFound();

            // PHÂN QUYỀN XEM CHI TIẾT
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                int? maKhSession = HttpContext.Session.GetInt32("MaKh");

                // Khách hàng chỉ được xem đơn của chính mình
                if (donHang.MaKh != maKhSession)
                {
                    return RedirectToAction(nameof(LichSuGiaoDich));
                }
            }
            // Admin được xem tất cả (không cần else)

            return View(donHang);
        }

        // POST: DonHangs/CapNhatTrangThai (CHỈ DÀNH CHO ADMIN)
        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(int maDonHang, string trangThaiMoi)
        {
            // BẢO MẬT TUYỆT ĐỐI: Chỉ Admin mới được cập nhật
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            var donHang = await _context.DonHangs.FindAsync(maDonHang);

            if (donHang == null) return NotFound();

            donHang.TrangThai = trangThaiMoi;

            _context.Update(donHang);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = maDonHang });
        }

        // POST: DonHangs/HuyDonHang (DÀNH CHO KHÁCH HÀNG)
        public async Task<IActionResult> HuyDonHang(int id)
        {
            // 1. Kiểm tra đăng nhập
            int? maKhSession = HttpContext.Session.GetInt32("MaKh");
            if (maKhSession == null) return RedirectToAction("Login", "Account");

            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            // 2. Kiểm tra đơn hàng tồn tại và phải là của User này
            if (donHang == null || donHang.MaKh != maKhSession)
            {
                return RedirectToAction(nameof(LichSuGiaoDich));
            }

            // 3. Kiểm tra điều kiện hủy: Chỉ hủy được khi chưa xử lý
            // (Bao gồm cả đơn Online chưa thanh toán và đơn COD mới đặt)
            if (donHang.TrangThai == "Chờ thanh toán" || donHang.TrangThai == "Mới đặt (COD)")
            {
                donHang.TrangThai = "Đã hủy"; // Cập nhật trạng thái
                _context.Update(donHang);

                // 4. QUAN TRỌNG: HOÀN LẠI TỒN KHO
                foreach (var chiTiet in donHang.ChiTietDonHangs)
                {
                    var spChiTiet = await _context.SanPhamChiTiets.FindAsync(chiTiet.MaChiTiet);
                    if (spChiTiet != null)
                    {
                        spChiTiet.SoLuongTon += chiTiet.SoLuong; // Cộng lại số lượng vào kho
                        _context.Update(spChiTiet);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng đã được xử lý hoặc đang giao.";
            }

            return RedirectToAction(nameof(LichSuGiaoDich));
        }
    }
}