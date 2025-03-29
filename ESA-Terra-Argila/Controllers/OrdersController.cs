using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Enums;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ESA_Terra_Argila.Controllers
{
    public class OrdersController : Controller
    {

        private readonly ApplicationDbContext _context;
        private string? userId;
        private readonly UserManager<User> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            userId = _userManager.GetUserId(User);
        }

        public async Task<IActionResult> Cart()
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Material)
                        .ThenInclude(m => m.Images)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Material)
                        .ThenInclude(m => m.Category)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft);
            return View(order);
        }

        

    }
}
