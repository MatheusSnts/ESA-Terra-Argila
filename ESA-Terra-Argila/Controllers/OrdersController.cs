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
                    .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft);

            foreach (var oi in order.OrderItems)
            {
                await _context.Entry(oi.Item).Reference(i => i.Category).LoadAsync();
                await _context.Entry(oi.Item).Collection(i => i.Images).LoadAsync();
            }
            return View(order);
        }

        public async Task<IActionResult> BuyNow(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            // Buscar o item SEM usar Include diretamente na classe abstrata
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound("Item não encontrado.");
            }

            // Carregar as propriedades de navegação manualmente
            await _context.Entry(item).Reference(i => i.Category).LoadAsync();
            await _context.Entry(item).Collection(i => i.Images).LoadAsync();

            // Criar o pedido
            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItem>
        {
            new OrderItem
            {
                ItemId = item.Id,
                Quantity = 1
            }
        }
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return View("BuyNow", order);
        }


        [HttpPost]
        public async Task<IActionResult> AddQuantity([FromBody] AddQuantityRequestModel request)
        {
            if (request == null || request.Id <= 0)
            {
                return BadRequest("Dados inválidos.");
            }

            var item = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Item)
                .FirstOrDefaultAsync(oi => oi.Id == request.Id);

            if (item == null)
            {
                return NotFound("Item não encontrado.");
            }

            item.Quantity += request.Value;

            if (item.Quantity <= 0)
            {
                return BadRequest("Ação não permitida");
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Quantidade alterada!",
                quantity = item.Quantity,
                partial = item.GetTotal().ToString("C", new System.Globalization.CultureInfo("pt-PT")),
                total = item.Order.GetTotal().ToString("C", new System.Globalization.CultureInfo("pt-PT"))
            });
        }

        public async Task<IActionResult> GetCartItemCount()
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("Utilizador não encontrado.");
            }

            var count = await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.UserId == userId && oi.Order.Status == OrderStatus.Draft)
                .CountAsync();

            return Ok(new
            {
                success = true,
                count
            });
        }

        public async Task<IActionResult> DeleteItem(int? id)
        {
            var referer = Request.Headers.Referer.ToString();

            if (id == null)
            {
                if (string.IsNullOrEmpty(referer))
                {
                    return NotFound("Página não encontrada.");
                }
                return Redirect(referer);
            }

            var item = await _context.OrderItems
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (item != null)
            {
                _context.OrderItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            if (string.IsNullOrEmpty(referer))
            {
                return NotFound("Página não encontrada.");
            }

            return Redirect(referer);
        }

        public class AddQuantityRequestModel
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
    }
}
