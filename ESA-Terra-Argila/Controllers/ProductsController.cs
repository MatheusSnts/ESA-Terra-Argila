using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NuGet.Packaging;
using ESA_Terra_Argila.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using X.PagedList.Extensions;
using ESA_Terra_Argila.Enums;


namespace ESA_Terra_Argila.Controllers
{
    /// <summary>
    /// Controller responsável pelo gerenciamento de produtos.
    /// Requer autenticação para a maioria das ações, exceto quando explicitamente permitido.
    /// </summary>
    /// 
    [Authorize(Policy = "AcceptedByAdmin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private string? userId;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ProductsController> _logger;

        /// <summary>
        /// Construtor do ProductsController.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        /// <param name="userManager">Gerenciador de usuários</param>
        /// <param name="logger">Logger para registrar eventos</param>
        public ProductsController(ApplicationDbContext context, UserManager<User> userManager, ILogger<ProductsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
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
        /// Exibe a lista de produtos do usuário atual.
        /// </summary>
        /// <returns>View com a lista de produtos</returns>
        // GET: Products
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Items
                    .OfType<Product>()
                    .Where(p => p.UserId == userId && p.DeletedAt == null && p.User.DeletedAt == null)
                    .Include(p => p.Category)
                    .Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        /// <summary>
        /// Exibe a lista de produtos para todos os usuários, com opções de filtragem e paginação.
        /// Esta ação é acessível por qualquer usuário, mesmo não autenticado.
        /// </summary>
        /// <param name="page">Número da página atual</param>
        /// <param name="orderBy">Ordem de classificação (asc/desc)</param>
        /// <param name="priceMin">Preço mínimo para filtro</param>
        /// <param name="priceMax">Preço máximo para filtro</param>
        /// <param name="vendors">Lista de IDs de vendedores para filtro</param>
        /// <param name="search">Termo de busca</param>
        /// <returns>View com a lista de produtos filtrada e paginada</returns>
        [AllowAnonymous]
        public async Task<IActionResult> List(int? page, string? orderBy, float? priceMin, float? priceMax, List<string>? vendors, string? search)
        {
            var query = _context.Items
                .OfType<Product>()
                .Where(p => p.DeletedAt == null && p.User.DeletedAt == null)
                .Include(m => m.Category)
                .Include(m => m.User)
                .Include(m => m.Images)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }
            if (priceMin.HasValue)
            {
                query = query.Where(p => p.Price >= priceMin.Value);
            }
            if (priceMax.HasValue)
            {
                query = query.Where(p => p.Price <= priceMax.Value);
            }

            if (vendors != null && vendors.Any())
            {
                query = query.Where(p => vendors.Contains(p.UserId));
            }
            orderBy ??= "asc";
            if (orderBy == "asc")
            {
                query = query.OrderBy(m => m.Price);
            }
            else if (orderBy == "desc")
            {
                query = query.OrderByDescending(m => m.Price);
            }

            var pageNumber = page ?? 1;
            var productsPage = query.ToPagedList(pageNumber, 30);

            ViewBag.ProductsPage = productsPage;

            var users = await _context.Users.ToListAsync();
            var vendorsList = new List<User>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Vendor"))
                {
                    vendorsList.Add(user);
                }
            }

            ViewData["Vendors"] = new SelectList(vendorsList, "Id", "FullName", vendors);
            ViewData["SelectedVendors"] = vendors;
            ViewData["Search"] = search;
            return View();
        }

        /// <summary>
        /// Exibe os detalhes de um produto específico.
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <returns>View com os detalhes do produto ou NotFound se não existir</returns>
        // GET: Products/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Items
                .OfType<Product>()
                .Where(p => p.DeletedAt == null && p.User.DeletedAt == null)
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        /// <summary>
        /// Exibe o formulário para criação de um novo produto.
        /// </summary>
        /// <returns>View com o formulário de criação</returns>
        // GET: Products/Create
        public IActionResult Create()
        {
            var favoriteMaterials = _context.UserMaterialFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.Material)
                .ToList();

            ViewData["Categories"] = new SelectList(_context.Categories.Where(c => c.UserId == userId), "Id", "Name");
            ViewData["Tags"] = new SelectList(_context.Tags.Where(t => t.UserId == userId), "Id", "Name");
            ViewData["FavoriteMaterials"] = new SelectList(favoriteMaterials, "Id", "Name");
            ViewData["Units"] = UnitsHelper.GetUnitsSelectList();
            return View();
        }

        /// <summary>
        /// Processa o envio do formulário de criação de um novo produto.
        /// </summary>
        /// <param name="product">Dados do produto a ser criado</param>
        /// <param name="Images">Lista de imagens do produto</param>
        /// <param name="Tags">Lista de IDs de tags associadas ao produto</param>
        /// <param name="Materials">Lista de IDs de materiais utilizados no produto</param>
        /// <param name="MaterialsQty">Lista de quantidades dos materiais utilizados</param>
        /// <returns>Redireciona para Index em caso de sucesso ou retorna à View com os dados em caso de erro</returns>
        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("CategoryId,Name,Reference,Description,Price,Unit,IsSustainable")]
Product product,
            List<IFormFile> Images,
            List<int> Tags,
            List<int> Materials,
            List<float?> MaterialsQty
            )
        {
            if (ModelState.IsValid)
            {
                product.UserId = userId;
                product.CreatedAt = DateTime.UtcNow;
                if (Tags != null && Tags.Any())
                {
                    var selectedTags = await _context.Tags.Where(t => Tags.Contains(t.Id)).ToListAsync();
                    product.Tags = selectedTags;
                }
                _context.Add(product);
                await _context.SaveChangesAsync();

                if (Materials != null && Materials.Any())
                {
                    for (int i = 0; i < Materials.Count; i++)
                    {
                        if (Materials[i] > 0)
                        {
                            var productMaterial = new ProductMaterial
                            {
                                ProductId = product.Id,
                                MaterialId = Materials[i],
                                Quantity = MaterialsQty[i] ?? 0f
                            };

                            _context.ProductMaterials.Add(productMaterial);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                if (Images != null && Images.Count > 0)
                {
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{ImageHelper.ItemImagesFolder}{product.Id}");

                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    foreach (var file in Images)
                    {
                        if (file.Length > 0)
                        {
                            ItemImage productImage = await ImageHelper.SaveItemImage(file, product.Id, imagesFolder);

                            _context.ItemImages.Add(productImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Produto adicionado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Erro ao adicionar produto!";
            return View(product);
        }

        /// <summary>
        /// Exibe o formulário para edição de um produto existente.
        /// </summary>
        /// <param name="id">ID do produto a ser editado</param>
        /// <returns>View com o formulário de edição ou NotFound se o produto não existir</returns>
        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Items
                .OfType<Product>()
                .Where(p => p.DeletedAt == null && p.User.DeletedAt == null)
                .Include(p => p.Tags)
                .Include(p => p.Images)
                .Include(p => p.ProductMaterials)
                .ThenInclude(pm => pm.Material)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var allTags = _context.Tags.Where(t => t.UserId == userId);
            var selectedTagIds = product.Tags.Select(t => t.Id).ToList();
            ViewData["Categories"] = new SelectList(_context.Categories.Where(c => c.UserId == userId), "Id", "Name");
            ViewData["Tags"] = new SelectList(allTags, "Id", "Name", selectedTagIds);

            var favoriteMaterials = await _context.UserMaterialFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.Material)
                .ToListAsync();

            var productMaterials = product.ProductMaterials.Select(pm => pm.Material).ToList();

            var combinedMaterials = favoriteMaterials
                .Concat(productMaterials)
                .Distinct() // Evita duplicatas
                .ToList();

            ViewData["FavoriteMaterials"] = new SelectList(combinedMaterials, "Id", "Name");

            ViewData["ProductMaterials"] = product.ProductMaterials
                .Select(pm => new { MaterialId = pm.MaterialId, Quantity = pm.Stock }) // Garantir que Stock é usado corretamente
                .ToList();
            ViewData["Units"] = UnitsHelper.GetUnitsSelectList();

            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,UserId,CategoryId,Name,Reference,Description,Price,Unit,IsSustainable")] Product product,
            List<IFormFile> Images,
            List<int> Tags,
            List<int> Materials,
            List<float?> MaterialsQty
        )
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var foundProduct = await _context.Items
                .OfType<Product>()
                .Where(p => p.DeletedAt == null && p.User.DeletedAt == null)
                .Include(p => p.Tags)
                .Include(p => p.Images)
                .Include(p => p.ProductMaterials)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (foundProduct == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    foundProduct.Name = product.Name;
                    foundProduct.CategoryId = product.CategoryId;
                    foundProduct.Reference = product.Reference;
                    foundProduct.Description = product.Description;
                    foundProduct.Price = product.Price;
                    foundProduct.Unit = product.Unit;
                    foundProduct.IsSustainable = product.IsSustainable;

                    if (Tags != null && Tags.Any())
                    {
                        var selectedTags = await _context.Tags.Where(t => Tags.Contains(t.Id)).ToListAsync();
                        foundProduct.Tags.Clear();
                        foundProduct.Tags.AddRange(selectedTags);
                    }

                    if (Materials != null && Materials.Any())
                    {
                        foundProduct.ProductMaterials.Clear();

                        for (int i = 0; i < Materials.Count; i++)
                        {
                            if (Materials[i] > 0 && MaterialsQty[i] > 0)
                            {
                                var productMaterial = new ProductMaterial
                                {
                                    ProductId = foundProduct.Id,
                                    MaterialId = Materials[i],
                                    Quantity = MaterialsQty[i] ?? 0
                                };

                                foundProduct.ProductMaterials.Add(productMaterial);
                            }
                        }
                    }

                    if (Images != null && Images.Count > 0)
                    {
                        var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{ImageHelper.ItemImagesFolder}{foundProduct.Id}");

                        foreach (var oldImage in foundProduct.Images.ToList())
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImage.Path.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            _context.ItemImages.Remove(oldImage);
                        }

                        await _context.SaveChangesAsync();

                        if (!Directory.Exists(imagesFolder))
                            Directory.CreateDirectory(imagesFolder);

                        foreach (var file in Images)
                        {
                            if (file.Length > 0)
                            {
                                var productImage = await ImageHelper.SaveItemImage(file, foundProduct.Id, imagesFolder);
                                _context.ItemImages.Add(productImage);
                            }
                        }
                    }

                    _context.Update(foundProduct);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Produto atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Erro ao editar produto!";
                }
            }

            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Items
                .OfType<Product>()
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            product.DeletedAt = DateTime.UtcNow;
            _context.Update(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Produto removido com sucesso!";
            return RedirectToAction("Index");

            //return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Items.FindAsync(id);
            if (product != null)
            {
                _context.Items.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
        public async Task<IActionResult> StockHistory(int id)
        {
            var product = await _context.Items
                .OfType<Product>()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Produto não encontrado!";
                return NotFound();
            }

            var movements = await _context.StockMovements
                .Where(m => m.ItemId == id)
                .OrderByDescending(m => m.Date)
                .Include(m => m.User)
                .ToListAsync();

            ViewData["Product"] = product;
            return View(movements);
        }

        public IActionResult CreateStockMovement(int id)
        {
            var product = _context.Items
                .OfType<Product>()
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Produto não encontrado!";
                return RedirectToAction(nameof(Index));
            }

            var movement = new StockMovement
            {
                ItemId = product.Id
            };

            return View(movement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStockMovement([Bind("ItemId,Quantity,Type")] StockMovement movement)
        {
            _logger.LogInformation("Entrou no método CreateStockMovement");

            var product = await _context.Items
                .OfType<Product>()
                .FirstOrDefaultAsync(i => i.Id == movement.ItemId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Produto não encontrado!";
                return RedirectToAction(nameof(Index));
            }

            if (movement.Type == "Entrada")
            {
                product.Stock += movement.Quantity;
            }
            else if (movement.Type == "Saída")
            {
                if (product.Stock < movement.Quantity)
                {
                    TempData["ErrorMessage"] = "Estoque insuficiente!";
                    return View(movement);
                }
                product.Stock -= movement.Quantity;
            }

            movement.Date = DateTime.UtcNow;
            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Movimento registado: {movement.Type} de {movement.Quantity} para o produto {product.Name}.");

            TempData["SuccessMessage"] = "Movimentação de estoque registada!";
            return RedirectToAction("Index", "Products");
        }


    }
}

