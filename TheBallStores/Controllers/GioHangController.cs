using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Helpers;
using TheBallStores.Models;
using System.Linq;

namespace TheBallStores.Controllers
{
    public class GioHangController : Controller
    {
        private readonly TheballStoreContext _context;

        public GioHangController(TheballStoreContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng từ Session
        public List<GioHangItem> LayGioHang()
        {
            // FIX LỖI NULL: Đảm bảo Session được xử lý an toàn
            var cart = HttpContext.Session.GetObjectFromJson<List<GioHangItem>>("GioHang");
            // Luôn trả về List rỗng nếu không tìm thấy
            return cart ?? new List<GioHangItem>();
        }

        // Trang Xem Giỏ Hàng (Khi click vào icon Giỏ hàng)
        public IActionResult Index()
        {
            // BẢO MẬT: Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = LayGioHang();
            // Tính tổng tiền
            ViewBag.TongTien = cart.Sum(item => item.ThanhTien);
            return View(cart);
        }

        // Hàm Thêm vào giỏ (Được gọi từ trang Chi tiết)
        [HttpPost]
        public async Task<IActionResult> ThemVaoGio(int maSp, int maSize, int soLuong)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = LayGioHang();
            var item = cart.FirstOrDefault(p => p.MaSp == maSp && p.MaSize == maSize);

            if (item != null)
            {
                // Nếu có rồi thì tăng số lượng
                item.SoLuong += soLuong;
            }
            else
            {
                // Nếu chưa có thì truy vấn DB lấy thông tin và thêm mới
                var sp = await _context.SanPhams.FindAsync(maSp);
                var size = await _context.KichThuocs.FindAsync(maSize);

                if (sp != null && size != null)
                {
                    // FIX LỖI NULL: Đã khởi tạo string.Empty trong GioHangItem.cs
                    item = new GioHangItem
                    {
                        MaSp = sp.MaSp,
                        TenSp = sp.TenSp,
                        AnhDaiDien = sp.AnhDaiDien,
                        DonGia = sp.GiaBan,
                        MaSize = size.MaSize,
                        TenSize = size.TenSize,
                        SoLuong = soLuong
                    };
                    cart.Add(item);
                }
            }

            // Lưu ngược lại vào Session
            HttpContext.Session.SetObjectAsJson("GioHang", cart);

            // Chuyển hướng đến trang Xem Giỏ Hàng
            return RedirectToAction("Index");
        }

        // HÀM XÓA KHỎI GIỎ HÀNG
        public IActionResult XoaKhoiGio(int maSp, int maSize)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = LayGioHang();

            // Tìm sản phẩm cần xóa theo Mã SP và Mã Size
            var itemToRemove = cart.FirstOrDefault(p => p.MaSp == maSp && p.MaSize == maSize);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }

            // Lưu lại vào Session
            HttpContext.Session.SetObjectAsJson("GioHang", cart);

            // Quay lại trang Giỏ hàng
            return RedirectToAction("Index");
        }

        // HÀM CẬP NHẬT SỐ LƯỢNG
        [HttpPost]
        public IActionResult CapNhatSoLuong(int maSp, int maSize, int soLuongMoi)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = LayGioHang();
            var item = cart.FirstOrDefault(p => p.MaSp == maSp && p.MaSize == maSize);

            if (item != null)
            {
                if (soLuongMoi > 0)
                {
                    item.SoLuong = soLuongMoi;
                }
                else
                {
                    // Nếu số lượng <= 0, coi như xóa luôn
                    cart.Remove(item);
                }
                HttpContext.Session.SetObjectAsJson("GioHang", cart);
            }

            // Chuyển hướng về trang Giỏ hàng
            return RedirectToAction("Index");
        }
    }
}