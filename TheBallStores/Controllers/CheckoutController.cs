using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Helpers; // Cần namespace này để dùng GetObjectFromJson
using TheBallStores.Models;
using System.Linq;

namespace TheBallStores.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly TheballStoreContext _context;

        public CheckoutController(TheballStoreContext context)
        {
            _context = context;
        }

        // HÀM NỘI BỘ LẤY GIỎ HÀNG (Sử dụng HttpContext của chính nó)
        private List<GioHangItem> LayGioHangTuSession()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<GioHangItem>>("GioHang");
            return cart ?? new List<GioHangItem>();
        }


        // GET: Checkout/Index (Trang xác nhận thông tin giao hàng)
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = HttpContext.Session.GetString("Email");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Logout", "Account");
            }

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == userEmail);

            if (khachHang == null)
            {
                return RedirectToAction("Logout", "Account");
            }

            var cart = LayGioHangTuSession();

            if (!cart.Any())
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
        public async Task<IActionResult> Confirm(KhachHang khachHang, string HinhThucThanhToan) // NHẬN THÊM THAM SỐ
        {
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = LayGioHangTuSession();

            if (!cart.Any())
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

                // LƯU TRẠNG THÁI DỰA TRÊN HÌNH THỨC THANH TOÁN
                TrangThai = HinhThucThanhToan == "ONLINE" ? "Chờ thanh toán" : "Mới đặt (COD)",

                // FIX LỖI 1: Lấy DiaChiGiaoHang từ form POST (khachHang.DiaChiGiaoHang)
                TenNguoiNhan = khachHang.HoTen ?? originalKhachHang.HoTen ?? string.Empty,
                DiaChiGiaoHang = khachHang.DiaChi ?? originalKhachHang.DiaChi ?? string.Empty,
                SoDienThoaiNguoiNhan = khachHang.DienThoai ?? originalKhachHang.DienThoai ?? string.Empty
            };

            _context.Add(donHang);
            await _context.SaveChangesAsync(); // Lưu đơn hàng để có MaDonHang

            // 3. Tạo Chi tiết đơn hàng và giảm tồn kho
            foreach (var item in cart)
            {
                var sanPhamChiTiet = await _context.SanPhamChiTiets
                    .FirstOrDefaultAsync(ct => ct.MaSp == item.MaSp && ct.MaSize == item.MaSize);

                if (sanPhamChiTiet == null)
                {
                    TempData["Error"] = "Lỗi kho hàng: Không tìm thấy chi tiết sản phẩm/size.";
                    continue;
                }

                var chiTiet = new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaChiTiet = sanPhamChiTiet.MaChiTiet,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia
                };

                // GIẢM TỒN KHO
                if (sanPhamChiTiet.SoLuongTon >= item.SoLuong)
                {
                    sanPhamChiTiet.SoLuongTon -= item.SoLuong;
                    _context.Update(sanPhamChiTiet);
                }

                _context.Add(chiTiet);
            }
            await _context.SaveChangesAsync();

            // 4. Xóa giỏ hàng khỏi Session
            HttpContext.Session.Remove("GioHang");

            // 5. ĐIỀU HƯỚNG DỰA TRÊN HÌNH THỨC THANH TOÁN
            if (HinhThucThanhToan == "ONLINE")
            {
                return RedirectToAction("PaymentSuccess", new { orderId = donHang.MaDonHang });
            }
            else // COD
            {
                return RedirectToAction("OrderSuccess", new { orderId = donHang.MaDonHang });
            }
        }

        // GET: Checkout/OrderSuccess (Trang xác nhận COD)
        public IActionResult OrderSuccess(int orderId)
        {
            ViewBag.MaDonHang = orderId;
            return View();
        }

        // GET: Checkout/PaymentSuccess (Trang hiển thị QR và STK)
        public IActionResult PaymentSuccess(int orderId)
        {
            var donHang = _context.DonHangs.Find(orderId);

            ViewBag.TongTien = donHang?.TongTien ?? 0;
            ViewBag.MaDonHang = donHang?.MaDonHang ?? 0;

            return View();
        }
    }
}