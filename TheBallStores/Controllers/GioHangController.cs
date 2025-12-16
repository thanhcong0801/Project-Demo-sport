using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Helpers;
using TheBallStores.Models;
using System.Linq;
using System;
using System.Collections.Generic;

namespace TheBallStores.Controllers
{
    public class GioHangController : Controller
    {
        private readonly TheballStoreContext _context;

        public GioHangController(TheballStoreContext context)
        {
            _context = context;
        }

        // HÀM LẤY GIỎ TỪ SESSION (Giữ nguyên logic cũ)
        public List<GioHangItem> LayGioHang()
        {
            // FIX LỖI NULL: Đảm bảo Session được xử lý an toàn
            var cart = HttpContext.Session.GetObjectFromJson<List<GioHangItem>>("GioHang");
            // Luôn trả về List rỗng nếu không tìm thấy
            return cart ?? new List<GioHangItem>();
        }

        // --- TRANG GIỎ HÀNG (INDEX) - ĐÃ CẬP NHẬT LOGIC VOUCHER ---
        public IActionResult Index()
        {
            // BẢO MẬT: Kiểm tra đăng nhập (Giữ nguyên logic cũ)
            if (HttpContext.Session.GetString("HoTen") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = LayGioHang();

            // Thay đổi: Sử dụng ViewModel để chứa cả Voucher
            var viewModel = new GioHangViewModel
            {
                Items = cart,
                TongTienHang = cart.Sum(item => item.ThanhTien)
            };

            // Logic Mới: Kiểm tra Voucher trong Session
            var sessionVoucher = HttpContext.Session.GetString("VoucherCode");

            if (!string.IsNullOrEmpty(sessionVoucher))
            {
                ApplyDiscountLogic(viewModel, sessionVoucher);
            }
            else
            {
                viewModel.SoTienGiam = 0;
                viewModel.TongThanhToan = viewModel.TongTienHang;
            }

            // Logic Mới: Thông báo lỗi/thành công
            if (TempData["VoucherMessage"] != null)
            {
                viewModel.ThongBao = TempData["VoucherMessage"].ToString();
            }

            // ViewBag cũ cho tương thích (nếu cần)
            ViewBag.TongTien = viewModel.TongThanhToan;

            return View(viewModel);
        }

        // --- HÀM XỬ LÝ VOUCHER (MỚI) ---
        [HttpPost]
        public IActionResult ApDungVoucher(string maVoucher)
        {
            if (string.IsNullOrWhiteSpace(maVoucher))
            {
                TempData["VoucherMessage"] = "Vui lòng nhập mã voucher.";
                return RedirectToAction("Index");
            }

            var voucher = _context.Vouchers.FirstOrDefault(v => v.MaVoucher == maVoucher);
            var cart = LayGioHang();
            var tongTien = cart.Sum(item => item.ThanhTien);

            if (voucher == null)
            {
                TempData["VoucherMessage"] = "Mã voucher không tồn tại.";
                HttpContext.Session.Remove("VoucherCode");
            }
            else if (!voucher.KichHoat)
            {
                TempData["VoucherMessage"] = "Voucher này đang bị khóa.";
                HttpContext.Session.Remove("VoucherCode");
            }
            else if (DateTime.Now < voucher.NgayBatDau || DateTime.Now > voucher.NgayKetThuc)
            {
                TempData["VoucherMessage"] = "Voucher chưa bắt đầu hoặc đã hết hạn.";
                HttpContext.Session.Remove("VoucherCode");
            }
            else if (voucher.SoLuongConLai <= 0)
            {
                TempData["VoucherMessage"] = "Voucher đã hết lượt sử dụng.";
                HttpContext.Session.Remove("VoucherCode");
            }
            else if (tongTien < voucher.DonToiThieu)
            {
                TempData["VoucherMessage"] = $"Đơn hàng chưa đạt tối thiểu {voucher.DonToiThieu:N0}đ.";
                HttpContext.Session.Remove("VoucherCode");
            }
            else
            {
                HttpContext.Session.SetString("VoucherCode", voucher.MaVoucher);
                TempData["VoucherMessage"] = "Áp dụng voucher thành công!";
            }

            return RedirectToAction("Index");
        }

        public IActionResult HuyVoucher()
        {
            HttpContext.Session.Remove("VoucherCode");
            TempData["VoucherMessage"] = "Đã hủy áp dụng voucher.";
            return RedirectToAction("Index");
        }

        // Helper tính toán (Private)
        private void ApplyDiscountLogic(GioHangViewModel vm, string maVoucher)
        {
            var voucher = _context.Vouchers.FirstOrDefault(v => v.MaVoucher == maVoucher);
            if (voucher != null)
            {
                vm.MaVoucher = maVoucher;
                decimal giamGia = 0;

                if (voucher.LoaiGiamGia == 1) // %
                {
                    giamGia = vm.TongTienHang * (voucher.GiaTriGiam / 100);
                    if (voucher.GiamToiDa.HasValue && giamGia > voucher.GiamToiDa.Value)
                    {
                        giamGia = voucher.GiamToiDa.Value;
                    }
                }
                else // Tiền mặt
                {
                    giamGia = voucher.GiaTriGiam;
                }

                if (giamGia > vm.TongTienHang) giamGia = vm.TongTienHang;

                vm.SoTienGiam = giamGia;
                vm.TongThanhToan = vm.TongTienHang - giamGia;
            }
            else
            {
                vm.SoTienGiam = 0;
                vm.TongThanhToan = vm.TongTienHang;
                HttpContext.Session.Remove("VoucherCode");
            }
        }

        // --- CÁC HÀM CŨ (GIỮ NGUYÊN 100% LOGIC CỦA BẠN) ---

        [HttpPost]
        // SỬA LỖI 1: Bỏ async/await vì hàm dùng Find() đồng bộ
        public IActionResult ThemVaoGio(int maSp, int maSize, int soLuong)
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
                var sp = _context.SanPhams.Find(maSp);
                var size = _context.KichThuocs.Find(maSize);

                // SỬA LỖI 2: Kiểm tra null an toàn trước khi tạo GioHangItem
                if (sp != null && size != null)
                {
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

        // HÀM XÓA KHỎI GIỎ HÀNG (Giữ nguyên logic cũ)
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

        // HÀM CẬP NHẬT SỐ LƯỢNG (Giữ nguyên logic cũ)
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