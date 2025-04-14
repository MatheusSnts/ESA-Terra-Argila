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
            // Buscar o carrinho atual (Order com status Draft)
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft);

            // Se não existir um carrinho, criar um novo
            if (order == null)
            {
                order = new Order
                {
                    UserId = userId,
                    Status = OrderStatus.Draft,
                    OrderItems = new List<OrderItem>()
                };
                
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
            }
            // Se existir um carrinho, carregar informações relacionadas
            else if (order.OrderItems.Any())
            {
                foreach (var oi in order.OrderItems)
                {
                    await _context.Entry(oi.Item).Reference(i => i.Category).LoadAsync();
                    await _context.Entry(oi.Item).Collection(i => i.Images).LoadAsync();
                }
            }

            // Calcular sustentabilidade com base nas quantidades
            float total = 0;
            float sustentaveis = 0;
            float naoSustentaveis = 0;

            foreach (var i in order.OrderItems)
            {
                if (i.Item != null)
                {
                    total += i.Quantity;
                    if (i.Item.IsSustainable)
                        sustentaveis += i.Quantity;
                    else
                        naoSustentaveis += i.Quantity;
                }
            }

            ViewBag.SustentavelPercent = total > 0 ? (int)Math.Round((double)sustentaveis * 100 / total) : 0;
            ViewBag.NaoSustentavelPercent = total > 0 ? (int)Math.Round((double)naoSustentaveis * 100 / total) : 0;

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
            // Verificar se existe um carrinho (Order com status Draft)
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft);

            // Se não existir um carrinho ou se o carrinho não tiver itens, criar um novo
            if (order == null)
            {
                order = new Order
                {
                    UserId = userId,
                    Status = OrderStatus.Draft,
                    OrderItems = new HashSet<OrderItem>()
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Salvar para garantir que o Order tenha um Id
            }

            // Buscar o item a ser adicionado
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound("Produto não encontrado");

            // Verificar se o item já existe no carrinho
            var existingItem = order.OrderItems.FirstOrDefault(oi => oi.ItemId == id);
            if (existingItem != null)
            {
                // Se já existe, incrementar a quantidade
                existingItem.Quantity += 1;
            }
            else
            {
                // Se não existe, adicionar novo item ao carrinho
                var orderItem = new OrderItem
                {
                    ItemId = id,
                    Quantity = 1,
                    OrderId = order.Id // Garantir que o OrderId está definido
                };
                order.OrderItems.Add(orderItem);
            }

            // Salvar as alterações
            await _context.SaveChangesAsync();

            // Redirecionar para a página anterior ou para a página inicial
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

            foreach (var orderItem in order.OrderItems)
            {
                orderItem.Item.Stock -= orderItem.Quantity; 
   
            }
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
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems) 
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound("Pedido não encontrado.");
            }

            
            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrdersList");
        }


        public async Task<IActionResult> OrdersList()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .Where(o => o.UserId == userId && o.Status != OrderStatus.Draft)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
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
                return string.IsNullOrEmpty(referer) ? NotFound("Página não encontrada.") : Redirect(referer);
            }

            var item = await _context.OrderItems
                .Include(oi => oi.Order) 
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (item != null)
            {
                var order = item.Order; 

                _context.OrderItems.Remove(item);
                await _context.SaveChangesAsync();

                
                if (!order.OrderItems.Any())
                {
                    order.Status = OrderStatus.Canceled;
                    await _context.SaveChangesAsync();
                }
            }

            return string.IsNullOrEmpty(referer) ? NotFound("Página não encontrada.") : Redirect(referer);
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
