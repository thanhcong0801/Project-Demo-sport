using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBallStores.Models;

namespace TheBallStores.Controllers
{
    public class VouchersController : Controller
    {
        private readonly TheballStoreContext _context;

        public VouchersController(TheballStoreContext context)
        {
            _context = context;
        }

        // GET: Admin/Vouchers
        public async Task<IActionResult> Index()
        {
            // Sắp xếp voucher mới nhất lên đầu
            return View(await _context.Vouchers.OrderByDescending(v => v.NgayTao).ToListAsync());
        }

        // GET: Admin/Vouchers/Details/CODE123
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(m => m.MaVoucher == id);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        // GET: Admin/Vouchers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Vouchers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaVoucher,TenChuongTrinh,LoaiGiamGia,GiaTriGiam,GiamToiDa,DonToiThieu,SoLuongConLai,NgayBatDau,NgayKetThuc,KichHoat")] Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mã trùng (Primary Key là String)
                if (VoucherExists(voucher.MaVoucher))
                {
                    ModelState.AddModelError("MaVoucher", "Mã voucher này đã tồn tại. Vui lòng chọn mã khác.");
                    return View(voucher);
                }

                // Tự động gán ngày tạo
                voucher.NgayTao = DateTime.Now;

                _context.Add(voucher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        // GET: Admin/Vouchers/Edit/CODE123
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        // POST: Admin/Vouchers/Edit/CODE123
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaVoucher,TenChuongTrinh,LoaiGiamGia,GiaTriGiam,GiamToiDa,DonToiThieu,SoLuongConLai,NgayBatDau,NgayKetThuc,KichHoat,NgayTao")] Voucher voucher)
        {
            if (id != voucher.MaVoucher)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.MaVoucher))
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
            return View(voucher);
        }

        // GET: Admin/Vouchers/Delete/CODE123
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(m => m.MaVoucher == id);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        // POST: Admin/Vouchers/Delete/CODE123
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher != null)
            {
                _context.Vouchers.Remove(voucher);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VoucherExists(string id)
        {
            return _context.Vouchers.Any(e => e.MaVoucher == id);
        }
    }
}