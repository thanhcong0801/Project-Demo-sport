using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TheBallStores.Helpers; // Đảm bảo bạn đã đổi namespace trong VnPayLibrary.cs thành TheBallStores.Helpers
using TheBallStores.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace TheBallStores.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly TheballStoreContext _context;
        private readonly IConfiguration _configuration;

        public CheckoutController(TheballStoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private List<GioHangItem> LayGioHangTuSession()
        {
            return HttpContext.Session.GetObjectFromJson<List<GioHangItem>>("GioHang") ?? new List<GioHangItem>();
        }

        private async Task<(decimal SoTienGiam, string? MaVoucher, Voucher? VoucherObj)> TinhToanGiamGia(decimal tongTienHang)
        {
            var maVoucher = HttpContext.Session.GetString("VoucherCode");
            if (string.IsNullOrEmpty(maVoucher)) return (0, null, null);

            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.MaVoucher == maVoucher);

            if (voucher == null || !voucher.KichHoat ||
                DateTime.Now < voucher.NgayBatDau || DateTime.Now > voucher.NgayKetThuc ||
                voucher.SoLuongConLai <= 0 || tongTienHang < voucher.DonToiThieu)
            {
                return (0, null, null);
            }

            decimal giamGia = 0;
            if (voucher.LoaiGiamGia == 1) // Phần trăm
            {
                giamGia = tongTienHang * (voucher.GiaTriGiam / 100);
                if (voucher.GiamToiDa.HasValue && giamGia > voucher.GiamToiDa.Value)
                {
                    giamGia = voucher.GiamToiDa.Value;
                }
            }
            else // Tiền mặt
            {
                giamGia = voucher.GiaTriGiam;
            }

            if (giamGia > tongTienHang) giamGia = tongTienHang;

            return (giamGia, maVoucher, voucher);
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("HoTen") == null) return RedirectToAction("Login", "Account");

            var userEmail = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Logout", "Account");

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == userEmail);
            if (khachHang == null) return RedirectToAction("Logout", "Account");

            var cart = LayGioHangTuSession();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Store");
            }

            decimal tongTienHang = cart.Sum(item => item.ThanhTien);
            var result = await TinhToanGiamGia(tongTienHang);

            ViewBag.Cart = cart;
            ViewBag.TongTienHang = tongTienHang;
            ViewBag.GiamGia = result.SoTienGiam;
            ViewBag.TongThanhToan = tongTienHang - result.SoTienGiam;
            ViewBag.MaVoucher = result.MaVoucher;

            return View(khachHang);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(KhachHang khachHang, string HinhThucThanhToan)
        {
            if (HttpContext.Session.GetString("HoTen") == null) return RedirectToAction("Login", "Account");

            var cart = LayGioHangTuSession();
            if (!cart.Any()) return RedirectToAction("Index", "Store");

            var userEmail = HttpContext.Session.GetString("Email");
            var originalKhachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == userEmail);
            if (originalKhachHang == null) return RedirectToAction("Logout", "Account");

            decimal tongTienHang = cart.Sum(item => item.ThanhTien);
            var voucherResult = await TinhToanGiamGia(tongTienHang);
            decimal tongThanhToan = tongTienHang - voucherResult.SoTienGiam;

            var donHang = new DonHang
            {
                MaKh = originalKhachHang.MaKh,
                NgayDat = DateTime.Now,
                TongTien = tongThanhToan,
                SoTienGiam = voucherResult.SoTienGiam,
                MaVoucher = voucherResult.MaVoucher,
                TrangThai = "Chờ thanh toán",
                TenNguoiNhan = khachHang.HoTen ?? originalKhachHang.HoTen ?? string.Empty,
                DiaChiGiaoHang = khachHang.DiaChi ?? originalKhachHang.DiaChi ?? string.Empty,
                SoDienThoaiNguoiNhan = khachHang.DienThoai ?? originalKhachHang.DienThoai ?? string.Empty
            };

            if (HinhThucThanhToan == "COD") donHang.TrangThai = "Mới đặt (COD)";

            if (voucherResult.VoucherObj != null)
            {
                voucherResult.VoucherObj.SoLuongConLai--;
                _context.Update(voucherResult.VoucherObj);
            }

            _context.Add(donHang);
            await _context.SaveChangesAsync();

            foreach (var item in cart)
            {
                var sanPhamChiTiet = await _context.SanPhamChiTiets
                    .FirstOrDefaultAsync(ct => ct.MaSp == item.MaSp && ct.MaSize == item.MaSize);

                if (sanPhamChiTiet != null)
                {
                    var chiTiet = new ChiTietDonHang
                    {
                        MaDonHang = donHang.MaDonHang,
                        MaChiTiet = sanPhamChiTiet.MaChiTiet,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia
                    };

                    if (sanPhamChiTiet.SoLuongTon >= item.SoLuong)
                    {
                        sanPhamChiTiet.SoLuongTon -= item.SoLuong;
                        _context.Update(sanPhamChiTiet);
                    }
                    _context.Add(chiTiet);
                }
            }
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("GioHang");
            HttpContext.Session.Remove("VoucherCode");

            if (HinhThucThanhToan == "ONLINE")
            {
                // --- GỌI SANG VNPAY ---
                var vnPayModel = new VnPayLibrary();

                // Sử dụng toán tử ?? "" để đảm bảo không bị null
                vnPayModel.AddRequestData("vnp_Version", _configuration["VnPay:Version"] ?? "2.1.0");
                vnPayModel.AddRequestData("vnp_Command", _configuration["VnPay:Command"] ?? "pay");
                vnPayModel.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"] ?? "");
                vnPayModel.AddRequestData("vnp_Amount", ((long)donHang.TongTien * 100).ToString());
                vnPayModel.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnPayModel.AddRequestData("vnp_CurrCode", _configuration["VnPay:CurrCode"] ?? "VND");

                // Gọi hàm Utils.GetIpAddress được định nghĩa ở cuối file này
                vnPayModel.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(HttpContext));
                vnPayModel.AddRequestData("vnp_Locale", _configuration["VnPay:Locale"] ?? "vn");

                vnPayModel.AddRequestData("vnp_OrderInfo", "ThanhToanDonHang" + donHang.MaDonHang);
                vnPayModel.AddRequestData("vnp_OrderType", "other");

                var returnUrl = Url.Action("PaymentCallback", "Checkout", null, Request.Scheme);
                vnPayModel.AddRequestData("vnp_ReturnUrl", returnUrl ?? "");

                vnPayModel.AddRequestData("vnp_TxnRef", donHang.MaDonHang.ToString());

                string baseUrl = _configuration["VnPay:BaseUrl"] ?? "";
                string hashSecret = _configuration["VnPay:HashSecret"] ?? "";

                string paymentUrl = vnPayModel.CreateRequestUrl(baseUrl, hashSecret);
                return Redirect(paymentUrl);
            }
            else
            {
                return RedirectToAction("OrderSuccess", new { orderId = donHang.MaDonHang });
            }
        }

        // --- XỬ LÝ KẾT QUẢ TRẢ VỀ TỪ VNPAY ---
        public async Task<IActionResult> PaymentCallback()
        {
            var vnpay = new VnPayLibrary();
            // Lấy toàn bộ query parameters
            foreach (var (key, value) in Request.Query)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            // Xử lý an toàn khi chuyển đổi kiểu dữ liệu
            long orderId = 0;
            long.TryParse(vnpay.GetResponseData("vnp_TxnRef"), out orderId);

            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");

            // Sửa lỗi null reference ở vnp_SecureHash
            string vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString() ?? "";

            string hashSecret = _configuration["VnPay:HashSecret"] ?? "";
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, hashSecret);

            if (checkSignature)
            {
                var donHang = await _context.DonHangs.FindAsync((int)orderId);
                if (vnp_ResponseCode == "00")
                {
                    if (donHang != null)
                    {
                        donHang.TrangThai = "Đã thanh toán (VNPay)";
                        _context.Update(donHang);
                        await _context.SaveChangesAsync();
                    }
                    ViewBag.MaDonHang = orderId;
                    ViewBag.TongTien = donHang?.TongTien ?? 0;
                    return View("PaymentSuccess");
                }
                else
                {
                    if (donHang != null)
                    {
                        donHang.TrangThai = "Hủy (Lỗi thanh toán)";
                        _context.Update(donHang);
                        await _context.SaveChangesAsync();
                    }
                    TempData["Error"] = $"Lỗi thanh toán VNPay: {vnp_ResponseCode}";
                    return RedirectToAction("Index", "Store");
                }
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra trong quá trình xử lý (Sai chữ ký).";
                return RedirectToAction("Index", "Store");
            }
        }

        public IActionResult OrderSuccess(int orderId)
        {
            ViewBag.MaDonHang = orderId;
            return View();
        }

        public IActionResult PaymentSuccess(int orderId)
        {
            var donHang = _context.DonHangs.Find(orderId);
            ViewBag.TongTien = donHang?.TongTien ?? 0;
            ViewBag.MaDonHang = donHang?.MaDonHang ?? 0;
            return View();
        }
    }

    // Helper để lấy IP, đặt ở đây để Controller sử dụng ngay mà không cần tìm file khác
    public static class Utils
    {
        public static string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIp = context.Connection.RemoteIpAddress;
                if (remoteIp != null)
                {
                    if (remoteIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        remoteIp = System.Net.Dns.GetHostEntry(remoteIp).AddressList
                            .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    }
                    ipAddress = remoteIp?.ToString();
                }
            }
            catch (Exception ex)
            {
                return "Invalid IP: " + ex.Message;
            }
            return ipAddress ?? "127.0.0.1";
        }
    }
}