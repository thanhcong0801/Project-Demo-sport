using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // Cần cho xử lý file
using Microsoft.AspNetCore.Http; // Cần cho IFormFile
using System.IO; // Cần cho FileStream, Path
using System;
using System.Collections.Generic;

namespace TheBallStores.Controllers
{
    public class SanPhamsController : Controller
    {
        private readonly TheballStoreContext _context;
        // Inject để truy cập thư mục wwwroot
        private readonly IWebHostEnvironment _hostEnvironment;

        // SỬA FIX: Thêm IWebHostEnvironment vào constructor
        public SanPhamsController(TheballStoreContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // ==========================================
        // CÁC HÀM CƠ BẢN
        // ==========================================

        // GET: SanPhams
        public async Task<IActionResult> Index()
        {
            var theballStoreContext = _context.SanPhams.Include(s => s.MaLoaiNavigation);
            return View(await theballStoreContext.ToListAsync());
        }

        // GET: SanPhams/Details/5 (Dành cho Admin xem)
        public async Task<IActionResult> Details(int? id)
        {
            // BẢO MẬT: Giả định chỉ Admin mới xem được chi tiết này
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                // Nếu không phải Admin, chuyển hướng (Hoặc bạn có thể cho phép khách hàng xem)
                // return RedirectToAction("Index", "Store"); 
            }

            if (id == null) return NotFound();

            var sanPham = await _context.SanPhams
                .Include(s => s.MaLoaiNavigation)
                .Include(s => s.HinhAnhSanPhams) // THÊM: Lấy ảnh phụ
                .Include(s => s.SanPhamChiTiets)
                    .ThenInclude(kho => kho.MaSizeNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);

            if (sanPham == null) return NotFound();

            return View(sanPham);
        }

        // GET: SanPhams/Create
        public IActionResult Create()
        {
            ViewData["MaLoai"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai");
            return View();
        }

        // POST: SanPhams/Create (CẬP NHẬT: Xử lý file)
        [HttpPost]
        [ValidateAntiForgeryToken]
        // NHẬN ẢNH ĐẠI DIỆN VÀ ẢNH PHỤ
        public async Task<IActionResult> Create([Bind("MaSp,TenSp,GiaBan,MoTa,MaLoai")] SanPham sanPham, IFormFile? HinhAnhDaiDienMoi, List<IFormFile>? HinhAnhPhuMoi)
        {
            if (ModelState.IsValid)
            {
                // 1. XỬ LÝ ẢNH ĐẠI DIỆN
                if (HinhAnhDaiDienMoi != null && HinhAnhDaiDienMoi.Length > 0)
                {
                    sanPham.AnhDaiDien = await SaveImageFile(HinhAnhDaiDienMoi, "products");
                }
                else
                {
                    sanPham.AnhDaiDien = "default.jpg";
                }

                _context.Add(sanPham);
                await _context.SaveChangesAsync();

                // 2. XỬ LÝ ẢNH PHỤ
                if (HinhAnhPhuMoi != null && HinhAnhPhuMoi.Any())
                {
                    foreach (var file in HinhAnhPhuMoi)
                    {
                        if (file.Length > 0)
                        {
                            var tenFile = await SaveImageFile(file, "products");
                            _context.HinhAnhSanPhams.Add(new HinhAnhSanPham
                            {
                                MaSp = sanPham.MaSp,
                                DuongDanAnh = tenFile
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Đã thêm sản phẩm. Vui lòng nhập thông tin tồn kho (Size/SL).";
                return RedirectToAction("NhapKho", new { id = sanPham.MaSp });
            }

            ViewData["MaLoai"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
            return View(sanPham);
        }

        // GET: SanPhams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.SanPhams == null)
            {
                return NotFound();
            }

            // THÊM: Lấy sản phẩm kèm theo thông tin ảnh phụ
            var sanPham = await _context.SanPhams
                .Include(s => s.HinhAnhSanPhams)
                .FirstOrDefaultAsync(m => m.MaSp == id);

            if (sanPham == null)
            {
                return NotFound();
            }
            ViewData["MaLoai"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
            return View(sanPham);
        }

        // POST: SanPhams/Edit/5 (CẬP NHẬT: Xử lý file)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("MaSp,TenSp,GiaBan,AnhDaiDien,MoTa,MaLoai")] SanPham sanPham,
            IFormFile? HinhAnhDaiDienMoi,
            List<IFormFile>? HinhAnhPhuMoi,
            string[]? DeleteImagePaths)
        {
            if (id != sanPham.MaSp)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingSanPham = await _context.SanPhams
                        .Include(s => s.HinhAnhSanPhams)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.MaSp == id);

                    if (existingSanPham == null) return NotFound();

                    // 2. XỬ LÝ ẢNH ĐẠI DIỆN MỚI
                    if (HinhAnhDaiDienMoi != null && HinhAnhDaiDienMoi.Length > 0)
                    {
                        sanPham.AnhDaiDien = await SaveImageFile(HinhAnhDaiDienMoi, "products");
                        if (existingSanPham.AnhDaiDien != "default.jpg") DeleteImageFile(existingSanPham.AnhDaiDien, "products");
                    }
                    else
                    {
                        sanPham.AnhDaiDien = existingSanPham.AnhDaiDien;
                    }

                    // 3. XỬ LÝ XÓA ẢNH PHỤ CŨ
                    if (DeleteImagePaths != null && DeleteImagePaths.Length > 0)
                    {
                        var spToDeleteFrom = await _context.SanPhams
                            .Include(s => s.HinhAnhSanPhams)
                            .FirstOrDefaultAsync(s => s.MaSp == id);

                        if (spToDeleteFrom != null)
                        {
                            foreach (var path in DeleteImagePaths)
                            {
                                var hinhAnh = spToDeleteFrom.HinhAnhSanPhams.FirstOrDefault(h => h.DuongDanAnh == path);
                                if (hinhAnh != null)
                                {
                                    _context.HinhAnhSanPhams.Remove(hinhAnh);
                                    DeleteImageFile(path, "products");
                                }
                            }
                            await _context.SaveChangesAsync();
                        }
                    }

                    // 4. XỬ LÝ THÊM ẢNH PHỤ MỚI
                    if (HinhAnhPhuMoi != null && HinhAnhPhuMoi.Any())
                    {
                        foreach (var file in HinhAnhPhuMoi)
                        {
                            if (file.Length > 0)
                            {
                                var tenFile = await SaveImageFile(file, "products");
                                _context.HinhAnhSanPhams.Add(new HinhAnhSanPham
                                {
                                    MaSp = sanPham.MaSp,
                                    DuongDanAnh = tenFile
                                });
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(sanPham.MaSp)) return NotFound();
                    else throw;
                }
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaLoai"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
            return View(sanPham);
        }

        // GET: SanPhams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.SanPhams == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.MaLoaiNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // POST: SanPhams/Delete/5 (CẬP NHẬT: Logic xóa đa cấp an toàn)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.SanPhams == null)
            {
                return Problem("Entity set 'TheballStoreContext.SanPhams' is null.");
            }

            var sanPham = await _context.SanPhams
                   .Include(s => s.HinhAnhSanPhams)
                   .Include(s => s.SanPhamChiTiets)
                   .FirstOrDefaultAsync(s => s.MaSp == id);

            if (sanPham != null)
            {
                // BƯỚC 1: XÓA CÁC CHI TIẾT ĐƠN HÀNG (TRUNG GIAN) để tránh lỗi FK
                var maChiTietIds = sanPham.SanPhamChiTiets
                    .Where(ct => ct.MaChiTiet > 0)
                    .Select(ct => ct.MaChiTiet)
                    .ToList();

                if (maChiTietIds.Any())
                {
                    var chiTietDonHangsToDelete = await _context.ChiTietDonHangs
                        .Where(dh => maChiTietIds.Contains(dh.MaChiTiet ?? 0))
                        .ToListAsync();

                    if (chiTietDonHangsToDelete.Any())
                    {
                        _context.ChiTietDonHangs.RemoveRange(chiTietDonHangsToDelete);
                    }
                }

                // BƯỚC 2: XÓA CÁC SẢN PHẨM CHI TIẾT (TỒN KHO)
                if (sanPham.SanPhamChiTiets.Any())
                {
                    _context.SanPhamChiTiets.RemoveRange(sanPham.SanPhamChiTiets);
                }

                // Xóa file ảnh phụ và ảnh đại diện
                if (!string.IsNullOrEmpty(sanPham.AnhDaiDien))
                {
                    DeleteImageFile(sanPham.AnhDaiDien, "products");
                }

                foreach (var hinhAnh in sanPham.HinhAnhSanPhams)
                {
                    if (!string.IsNullOrEmpty(hinhAnh.DuongDanAnh))
                    {
                        DeleteImageFile(hinhAnh.DuongDanAnh, "products");
                    }
                }

                // BƯỚC CUỐI CÙNG: XÓA SẢN PHẨM GỐC
                _context.SanPhams.Remove(sanPham);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool SanPhamExists(int id)
        {
            return (_context.SanPhams?.Any(e => e.MaSp == id)).GetValueOrDefault();
        }

        // GET: SanPhams/NhapKho/5 (Hiển thị form nhập kho)
        public async Task<IActionResult> NhapKho(int? id)
        {
            if (id == null) return NotFound();

            var sanPham = await _context.SanPhams
                .Include(s => s.SanPhamChiTiets)
                    .ThenInclude(ct => ct.MaSizeNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);

            if (sanPham == null) return NotFound();

            // Lấy danh sách tất cả size có sẵn để Admin chọn
            ViewBag.AvailableSizes = await _context.KichThuocs.ToListAsync();

            return View(sanPham);
        }

        // POST: SanPhams/NhapKho (Xử lý lưu kho)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhapKho(int MaSp, int MaSize, int SoLuongTon)
        {
            var existingChiTiet = await _context.SanPhamChiTiets
                .FirstOrDefaultAsync(ct => ct.MaSp == MaSp && ct.MaSize == MaSize);

            if (existingChiTiet != null)
            {
                existingChiTiet.SoLuongTon = SoLuongTon;
                _context.Update(existingChiTiet);
            }
            else
            {
                var newChiTiet = new SanPhamChiTiet
                {
                    MaSp = MaSp,
                    MaSize = MaSize,
                    SoLuongTon = SoLuongTon
                };
                _context.Add(newChiTiet);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật tồn kho thành công!";
            return RedirectToAction(nameof(NhapKho), new { id = MaSp });
        }

        // ==========================================
        // HÀM HỖ TRỢ XỬ LÝ FILE (Đã được sửa an toàn)
        // ==========================================

        private async Task<string> SaveImageFile(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return string.Empty;

            string webRootPath = _hostEnvironment.WebRootPath;
            string uploadsFolder = Path.Combine(webRootPath, "images", folder);

            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
                return string.Empty;
            }

            return uniqueFileName;
        }

        private void DeleteImageFile(string? fileName, string folder) // Chấp nhận string?
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "default.jpg") return;

            string webRootPath = _hostEnvironment.WebRootPath;
            string filePath = Path.Combine(webRootPath, "images", folder, fileName);

            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file {fileName}: {ex.Message}");
            }
        }
    }
}