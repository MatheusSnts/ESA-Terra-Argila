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
    /// Controller responsável pelo gerenciamento de pedidos e carrinho de compras.
    /// </summary>
    public class OrdersController : Controller
    {

        private readonly ApplicationDbContext _context;
        private string? userId;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Construtor do OrdersController.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        /// <param name="userManager">Gerenciador de usuários</param>
        public OrdersController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Método executado antes de cada ação, para obter o ID do usuário atual.
        /// </summary>
        /// <param name="context">Contexto da execução da ação</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            userId = _userManager.GetUserId(User);
        }

        /// <summary>
        /// Exibe o carrinho de compras do usuário atual.
        /// Se não existir um carrinho, cria um novo.
        /// </summary>
        /// <returns>View com o carrinho de compras</returns>
        public async Task<IActionResult> Cart()
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft);

            var items = order?.OrderItems ?? new HashSet<OrderItem>();

            if (!items.Any())
            {
                order = new Order
                {
                    UserId = userId,
                    Status = OrderStatus.Draft,
                    OrderItems = new List<OrderItem>()
                };
            }
            else
            {
                foreach (var oi in order.OrderItems)
                {
                    await _context.Entry(oi.Item).Reference(i => i.Category).LoadAsync();
                    await _context.Entry(oi.Item).Collection(i => i.Images).LoadAsync();
                }
            }

            return View(order);
        }

        /// <summary>
        /// Adiciona um item ao carrinho de compras.
        /// Se o carrinho não existir, cria um novo.
        /// </summary>
        /// <param name="id">ID do item a ser adicionado</param>
        /// <returns>Redireciona para a página anterior ou para a página inicial</returns>
        public async Task<IActionResult> AddToCart(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft);

            if (order == null || !order.OrderItems.Any())
            {
                order = new Order
                {
                    UserId = userId,
                    Status = OrderStatus.Draft
                };
                _context.Orders.Add(order);
            }

            var material = await _context.Items.FindAsync(id);
            if (material == null)
                return NotFound("Produto não encontrado");
            var existingItem = order.OrderItems.FirstOrDefault(oi => oi.ItemId == id);
            if (existingItem != null)
            {
                existingItem.Quantity += 1;
            }
            else
            {
                var orderItem = new OrderItem
                {
                    ItemId = id,
                    Quantity = 1
                };
                order.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            var referer = Request.Headers.Referer.ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Inicia o processo de compra imediata para um item.
        /// (Método stub, não implementado completamente)
        /// </summary>
        /// <param name="id">ID do item a ser comprado</param>
        /// <returns>Resultado OK (método a ser implementado)</returns>
        public async Task<IActionResult> BuyNow(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

          
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound("Item não encontrado.");
            }

        
            await _context.Entry(item).Reference(i => i.Category).LoadAsync();
            await _context.Entry(item).Collection(i => i.Images).LoadAsync();

       
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

        /// <summary>
        /// Altera a quantidade de um item no carrinho de compras.
        /// </summary>
        /// <param name="request">Modelo com ID do item e valor a ser adicionado à quantidade</param>
        /// <returns>Resultado JSON com informações atualizadas</returns>
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

        /// <summary>
        /// Retorna o número de itens no carrinho de compras do usuário atual.
        /// </summary>
        /// <returns>Resultado JSON com a contagem de itens</returns>
        public async Task<IActionResult> GetCartItemCount()
        {
            if(string.IsNullOrEmpty(userId))
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

        /// <summary>
        /// Remove um item do carrinho de compras.
        /// </summary>
        /// <param name="id">ID do item a ser removido</param>
        /// <returns>Redireciona para a página anterior</returns>
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

        /// <summary>
        /// Modelo para a requisição de alteração de quantidade.
        /// </summary>
        public class AddQuantityRequestModel
        {
            /// <summary>
            /// ID do item no carrinho.
            /// </summary>
            public int Id { get; set; }
            
            /// <summary>
            /// Valor a ser adicionado à quantidade atual (pode ser positivo ou negativo).
            /// </summary>
            public int Value { get; set; }
        }
    }
}
