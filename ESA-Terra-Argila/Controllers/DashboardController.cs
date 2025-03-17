using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESA_Terra_Argila.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole("Vendor"))
            {
                return RedirectToRoute(new { controller = "Products", action = "Index" });
            }
            else if (User.IsInRole("Supplier"))
            {
                return RedirectToRoute(new { controller = "Materials", action = "Index" });
            }
            else if (User.IsInRole("Admin"))
            {
                return RedirectToRoute(new { controller = "Admin", action = "AcceptUsers" });
            }
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }
    }
}
