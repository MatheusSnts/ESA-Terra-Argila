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
    /// <summary>
    /// Controlador responsável por gerenciar os pedidos no sistema.
    /// Permite criar, visualizar e gerenciar o carrinho de compras dos usuários.
    /// </summary>
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private string? userId;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Inicializa uma nova instância do controlador de pedidos.
        /// </summary>
        /// <param name="context">O contexto do banco de dados da aplicação.</param>
        /// <param name="userManager">O gerenciador de usuários do Identity.</param>
        public OrdersController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Executa antes de cada ação do controlador para definir o ID do usuário atual.
        /// </summary>
        /// <param name="context">O contexto da execução da ação.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            userId = _userManager.GetUserId(User);
        }

        /// <summary>
        /// Exibe o carrinho de compras do usuário atual.
        /// </summary>
        /// <returns>A view do carrinho de compras com os itens adicionados.</returns>
        public async Task<IActionResult> Cart()
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft);

            if (order == null)
            {
                return View(new Order
                {
                    UserId = userId,
                    Status = OrderStatus.Draft,
                    OrderItems = new List<OrderItem>()
                });
            }

            foreach (var oi in order.OrderItems)
            {
                if (oi.Item != null)
                {
                    await _context.Entry(oi.Item).Reference(i => i.Category).LoadAsync();
                    await _context.Entry(oi.Item).Collection(i => i.Images).LoadAsync();
                }
            }
            return View(order);
        }

        /// <summary>
        /// Adiciona um item ao carrinho de compras do usuário.
        /// </summary>
        /// <param name="id">O ID do item a ser adicionado ao carrinho.</param>
        /// <returns>Redireciona de volta à página anterior após adicionar o item.</returns>
        public async Task<IActionResult> AddToCart(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft);

            if (order == null)
            {
                order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = OrderStatus.Draft
                };
                _context.Orders.Add(order);
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                TempData["ErrorMessage"] = "Item não encontrado!";
                return RedirectToAction("Index", "Home");
            }

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ItemId == id);
            if (orderItem == null)
            {
                orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ItemId = id,
                    Quantity = 1,
                    UnitPrice = (decimal)item.Price 

                };
                order.OrderItems.Add(orderItem);
            }
            else
            {
                orderItem.Quantity++;
            }

            orderItem.TotalPrice = (decimal)orderItem.Quantity * (decimal)item.Price;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Item adicionado ao carrinho!";

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Remove um item do carrinho de compras do usuário.
        /// </summary>
        /// <param name="id">O ID do item a ser removido do carrinho.</param>
        /// <returns>Redireciona de volta à página anterior após remover o item.</returns>
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Carrinho não encontrado!";
                return RedirectToAction("Index", "Home");
            }

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ItemId == id);
            if (orderItem == null)
            {
                TempData["ErrorMessage"] = "Item não encontrado no carrinho!";
                return RedirectToAction("Index", "Home");
            }

            order.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Item removido do carrinho!";

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Atualiza a quantidade de um item no carrinho de compras.
        /// </summary>
        /// <param name="id">O ID do item a ser atualizado.</param>
        /// <param name="value">A nova quantidade do item.</param>
        /// <returns>Redireciona de volta à página anterior após atualizar a quantidade.</returns>
        public async Task<IActionResult> AddQuantity(int id, int value)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Carrinho não encontrado!";
                return RedirectToAction("Index", "Home");
            }

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ItemId == id);
            if (orderItem == null)
            {
                TempData["ErrorMessage"] = "Item não encontrado no carrinho!";
                return RedirectToAction("Index", "Home");
            }

            orderItem.Quantity += value;
            if (orderItem.Quantity <= 0)
            {
                order.OrderItems.Remove(orderItem);
            }
            else
            {
                orderItem.TotalPrice = (decimal)orderItem.Quantity * (decimal)orderItem.UnitPrice;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Quantidade atualizada!";

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Modelo de requisição para atualizar a quantidade de um item.
        /// </summary>
        public class AddQuantityRequestModel
        {
            /// <summary>
            /// O ID do item a ser atualizado.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// O valor a ser adicionado à quantidade atual.
            /// </summary>
            public int Value { get; set; }
        }
    }
}
