using Microsoft.AspNetCore.Mvc;
using TheBallStores.Helpers;
using TheBallStores.Models;

namespace TheBallStores.ViewComponents
{
    public class CartBadgeViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<GioHangItem>>("GioHang");
            int totalItems = cart != null ? cart.Sum(item => item.SoLuong) : 0;
            return View(totalItems);
        }
    }
}