using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Helpers;
using TheBallStores.Models;
using System.Linq; // Cần dùng cho .Any() và .Sum()

namespace TheBallStores.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly TheballStoreContext _context;

        public CheckoutController(TheballStoreContext context)
        {
            _context = context;
        }

        // GET: Checkout/Index (Trang xác nhận thông tin giao hàng)
        public async Task<IActionResult> Index()
        {
            // BẢO MẬT: Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = HttpContext.Session.GetString("Email");

            // Kiểm tra email có tồn tại không
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Logout", "Account");
            }

            // Lấy thông tin khách hàng từ Database
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == userEmail);

            if (khachHang == null)
            {
                return RedirectToAction("Logout", "Account");
            }

            // Lấy giỏ hàng để kiểm tra
            var gioHangController = new GioHangController(_context);
            var cart = gioHangController.LayGioHang();

            if (cart == null || !cart.Any()) // FIX LỖI: Sử dụng .Any() để kiểm tra giỏ hàng
            {
                TempData["Error"] = "Giỏ hàng trống! Vui lòng chọn sản phẩm.";
                return RedirectToAction("Index", "Store");
            }

            ViewBag.Cart = cart;
            ViewBag.TongTien = cart.Sum(item => item.ThanhTien);

            return View(khachHang);
        }

        // POST: Checkout/Confirm (Xử lý lưu Đơn hàng và chuyển sang trang Thanh toán)
        [HttpPost]
        public async Task<IActionResult> Confirm(KhachHang khachHang)
        {
            // BẢO MẬT: Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 1. Lấy thông tin giỏ hàng
            var gioHangController = new GioHangController(_context);
            var cart = gioHangController.LayGioHang();

            if (cart == null || !cart.Any()) // Kiểm tra giỏ hàng rỗng lần nữa
            {
                TempData["Error"] = "Giỏ hàng trống! Không thể đặt hàng.";
                return RedirectToAction("Index", "Store");
            }

            // Lấy lại Khách hàng gốc từ DB (để đảm bảo MaKH là chính xác)
            var originalKhachHang = await _context.KhachHangs.FindAsync(khachHang.MaKh);
            if (originalKhachHang == null)
            {
                return RedirectToAction("Logout", "Account");
            }

            // 2. Tạo đơn hàng mới
            var donHang = new DonHang
            {
                MaKh = originalKhachHang.MaKh,
                NgayDat = DateTime.Now,
                TongTien = cart.Sum(item => item.ThanhTien),
                TrangThai = "Mới đặt",

                // FIX LỖI NULL: Sử dụng ?? string.Empty để đảm bảo giá trị string không null
                TenNguoiNhan = khachHang.HoTen ?? originalKhachHang.HoTen ?? string.Empty,
                DiaChiGiaoHang = khachHang.DiaChi ?? originalKhachHang.DiaChi ?? string.Empty,
                SoDienThoaiNguoiNhan = khachHang.DienThoai ?? originalKhachHang.DienThoai ?? string.Empty
            };

            _context.Add(donHang);
            await _context.SaveChangesAsync(); // Lưu đơn hàng để có MaDonHang

            // 3. Tạo Chi tiết đơn hàng
            foreach (var item in cart)
            {
                // BƯỚC 3.1: TÌM MaChiTiet của SanPhamChiTiets dựa trên MaSp và MaSize
                var sanPhamChiTiet = await _context.SanPhamChiTiets
                    .FirstOrDefaultAsync(ct => ct.MaSp == item.MaSp && ct.MaSize == item.MaSize);

                if (sanPhamChiTiet == null)
                {
                    TempData["Error"] = "Lỗi kho hàng: Không tìm thấy chi tiết sản phẩm/size.";
                    // Giữ lại item trong giỏ hàng để khách biết bị lỗi
                    continue;
                }

                var chiTiet = new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaChiTiet = sanPhamChiTiet.MaChiTiet, // LƯU ID CHÍNH XÁC CỦA SP_CHI_TIET
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia
                };

                // BƯỚC 3.2: GIẢM TỒN KHO
                sanPhamChiTiet.SoLuongTon -= item.SoLuong;
                _context.Update(sanPhamChiTiet);

                _context.Add(chiTiet);
            }
            await _context.SaveChangesAsync();

            // 4. Xóa giỏ hàng khỏi Session
            HttpContext.Session.Remove("GioHang");

            // Chuyển hướng đến trang Thanh toán
            return RedirectToAction("PaymentSuccess", new { orderId = donHang.MaDonHang });
        }

        // GET: Checkout/PaymentSuccess (Trang hiển thị QR và STK)
        public IActionResult PaymentSuccess(int orderId)
        {
            // Lấy lại tổng tiền cần chuyển khoản (Từ DB hoặc tính lại)
            var donHang = _context.DonHangs.Find(orderId);

            ViewBag.TongTien = donHang?.TongTien ?? 0;
            ViewBag.MaDonHang = donHang?.MaDonHang ?? 0;

            return View();
        }
    }
}