using Microsoft.AspNetCore.Mvc;
using TheBallStores.Helpers;
using TheBallStores.Models;

namespace TheBallStores.ViewComponents
{
    public class CartBadgeViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            // Lấy giỏ hàng từ Session Helper
            var cart = HttpContext.Session.GetObjectFromJson<List<GioHangItem>>("GioHang");

            // Tính tổng số lượng (hoặc tổng loại sản phẩm)
            int totalItems = cart != null ? cart.Sum(item => item.SoLuong) : 0;

            // Gửi số lượng sang View
            return View(totalItems);
        }
    }
}