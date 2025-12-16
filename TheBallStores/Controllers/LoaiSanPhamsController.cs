using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace TheBallStores.Controllers
{
    public class LoaiSanPhamsController : Controller
    {
        private readonly TheballStoreContext _context;

        public LoaiSanPhamsController(TheballStoreContext context)
        {
            _context = context;
        }

        // GET: LoaiSanPhams (Hiển thị danh sách)
        public async Task<IActionResult> Index()
        {
            // Yêu cầu quyền Admin (Nếu cần)
            // if (HttpContext.Session.GetString("VaiTro") != "Admin") return RedirectToAction("Index", "Home");

            return View(await _context.LoaiSanPhams.ToListAsync());
        }

        // ===========================================
        // BỔ SUNG: TẠO NHANH DANH MỤC QUA AJAX
        // ===========================================

        // POST: LoaiSanPhams/CreateAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax(string TenLoai, string? NhomLoai)
        {
            if (string.IsNullOrWhiteSpace(TenLoai))
            {
                return Json(new { success = false, message = "Tên loại sản phẩm là bắt buộc." });
            }

            try
            {
                var loaiSanPham = new LoaiSanPham
                {
                    TenLoai = TenLoai,
                    NhomLoai = NhomLoai
                };

                _context.Add(loaiSanPham);
                await _context.SaveChangesAsync();

                return Json(new { success = true, maLoai = loaiSanPham.MaLoai, tenLoai = loaiSanPham.TenLoai });
            }
            catch (System.Exception ex)
            {
                // Ghi log lỗi để debug
                System.Console.WriteLine($"Error creating category via AJAX: {ex.Message}");
                return Json(new { success = false, message = "Lỗi server khi lưu danh mục." });
            }
        }

        // GET: LoaiSanPhams/GetLoaiSanPhams (Trả về danh sách dưới dạng JSON cho AJAX)
        public async Task<IActionResult> GetLoaiSanPhams()
        {
            var data = await _context.LoaiSanPhams
                .Select(l => new {
                    value = l.MaLoai,
                    text = l.TenLoai
                })
                .OrderBy(l => l.text)
                .ToListAsync();

            return Json(data);
        }

        // ===========================================
        // END BỔ SUNG
        // ===========================================


        // GET: LoaiSanPhams/Create (Giữ nguyên form Create ban đầu)
        public IActionResult Create()
        {
            return View();
        }

        // POST: LoaiSanPhams/Create (Giữ nguyên Post Create ban đầu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenLoai,NhomLoai")] LoaiSanPham loaiSanPham)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loaiSanPham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(loaiSanPham);
        }

        // GET: LoaiSanPhams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var loaiSanPham = await _context.LoaiSanPhams.FindAsync(id);
            if (loaiSanPham == null) return NotFound();

            return View(loaiSanPham);
        }

        // POST: LoaiSanPhams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaLoai,TenLoai,NhomLoai")] LoaiSanPham loaiSanPham)
        {
            if (id != loaiSanPham.MaLoai) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaiSanPham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.LoaiSanPhams.Any(e => e.MaLoai == loaiSanPham.MaLoai))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(loaiSanPham);
        }

        // GET: LoaiSanPhams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var loaiSanPham = await _context.LoaiSanPhams
                .FirstOrDefaultAsync(m => m.MaLoai == id);

            if (loaiSanPham == null) return NotFound();

            // Kiểm tra ràng buộc FK: Nếu có sản phẩm nào thuộc danh mục này, không cho phép xóa
            if (_context.SanPhams.Any(s => s.MaLoai == id))
            {
                TempData["ErrorMessage"] = "Không thể xóa danh mục này vì vẫn còn sản phẩm thuộc về nó.";
                return RedirectToAction(nameof(Index));
            }

            return View(loaiSanPham);
        }

        // POST: LoaiSanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int maLoai)
        {
            var loaiSanPham = await _context.LoaiSanPhams.FindAsync(maLoai);
            if (loaiSanPham != null)
            {
                _context.LoaiSanPhams.Remove(loaiSanPham);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}