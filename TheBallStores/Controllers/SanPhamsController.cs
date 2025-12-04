using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TheBallStores.Controllers
{
    public class SanPhamsController : Controller
    {
        private readonly TheballStoreContext _context;

        public SanPhamsController(TheballStoreContext context)
        {
            _context = context;
        }

        // GET: SanPhams
        public async Task<IActionResult> Index()
        {
            // Nối bảng SanPhams với LoaiSanPhams (MaLoaiNavigation) để hiển thị Tên Loại
            var theballStoreContext = _context.SanPhams.Include(s => s.MaLoaiNavigation);
            return View(await theballStoreContext.ToListAsync());
        }

        // GET: SanPhams/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: SanPhams/Create
        public IActionResult Create()
        {
            // Nạp danh sách Loại sản phẩm vào ViewData để dùng trong View
            ViewData["MaLoai"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai");
            return View();
        }

        // POST: SanPhams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSp,TenSp,GiaBan,MoTa,AnhDaiDien,MaLoai")] SanPham sanPham)
        {
            if (ModelState.IsValid)
            {
                // Tên file ảnh đại diện mặc định
                sanPham.AnhDaiDien = "default.jpg";

                _context.Add(sanPham);
                await _context.SaveChangesAsync();

                // QUAN TRỌNG: Chuyển hướng đến trang nhập kho (Size/SL)
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

            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return NotFound();
            }
            ViewData["MaLoai"] = new SelectList(_context.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
            return View(sanPham);
        }

        // POST: SanPhams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaSp,TenSp,GiaBan,AnhDaiDien,MoTa,MaLoai")] SanPham sanPham)
        {
            if (id != sanPham.MaSp)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(sanPham.MaSp))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
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

        // POST: SanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.SanPhams == null)
            {
                return Problem("Entity set 'TheballStoreContext.SanPhams' is null.");
            }
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham != null)
            {
                _context.SanPhams.Remove(sanPham);
            }

            await _context.SaveChangesAsync();
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
                // Cập nhật số lượng (Nếu đã có size này)
                existingChiTiet.SoLuongTon = SoLuongTon;
                _context.Update(existingChiTiet);
            }
            else
            {
                // Thêm mới (Nếu chưa có size này)
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
    }
}