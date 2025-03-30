using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ESA_Terra_Argila.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private string? userId;
        private readonly UserManager<User> _userManager;



        public HomeController(ApplicationDbContext context, UserManager<User> userManager, ILogger<HomeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;

        }


        public IActionResult Index()
        {
            var topProductIds = _context.OrderItems
                .Where(oi => oi.Item is Product)
                .GroupBy(oi => oi.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key.Value,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .Select(x => x.ItemId)
                .ToList();

            var top4Products = _context.Items
                .OfType<Product>()
                .Where(p => topProductIds.Contains(p.Id))
                .Include(p => p.Images)
                .Include(p => p.User)
                .ToList();

            return View(top4Products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
