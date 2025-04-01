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
    /// Controlador responsável por gerenciar os produtos no sistema.
    /// Permite criar, editar, excluir e visualizar produtos, além de gerenciar sua composição com materiais.
    /// </summary>
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private string? userId;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Inicializa uma nova instância do controlador de produtos.
        /// </summary>
        /// <param name="context">O contexto do banco de dados da aplicação.</param>
        /// <param name="userManager">O gerenciador de usuários do Identity.</param>
        public ProductsController(ApplicationDbContext context, UserManager<User> userManager)
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
        /// Exibe a lista de produtos do usuário atual.
        /// </summary>
        /// <returns>Uma view contendo a lista de produtos.</returns>
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Items
                    .OfType<Product>()
                    .Where(p => p.UserId == userId)
                    .Include(p => p.Category)
                    .Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        /// <summary>
        /// Exibe uma lista paginada de produtos disponíveis para compra.
        /// </summary>
        /// <param name="page">Número da página a ser exibida.</param>
        /// <param name="orderBy">Ordem de classificação dos produtos (asc/desc).</param>
        /// <param name="priceMin">Preço mínimo para filtrar produtos.</param>
        /// <param name="priceMax">Preço máximo para filtrar produtos.</param>
        /// <param name="vendors">Lista de IDs dos vendedores para filtrar produtos.</param>
        /// <param name="search">Termo de busca para filtrar produtos por nome.</param>
        /// <returns>Uma view contendo a lista filtrada e paginada de produtos.</returns>
        [AllowAnonymous]
        public async Task<IActionResult> List(int? page, string? orderBy, float? priceMin, float? priceMax, List<string>? vendors, string? search)
        {
            var query = _context.Items
                .OfType<Product>()
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
        /// <param name="id">O ID do produto a ser exibido.</param>
        /// <returns>A view com os detalhes do produto ou NotFound se não encontrado.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Items
                .OfType<Product>()
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
        /// Exibe o formulário para criar um novo produto.
        /// </summary>
        /// <returns>A view do formulário de criação com as listas de categorias, tags e materiais favoritos.</returns>
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
        /// Cria um novo produto no sistema.
        /// </summary>
        /// <param name="product">Os dados do produto a ser criado.</param>
        /// <param name="Images">Lista de imagens do produto.</param>
        /// <param name="Tags">Lista de IDs das tags do produto.</param>
        /// <param name="Materials">Lista de IDs dos materiais usados no produto.</param>
        /// <param name="MaterialsQty">Lista de quantidades dos materiais usados.</param>
        /// <returns>Redireciona para a lista de produtos se bem-sucedido, ou retorna a view com erros se falhar.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("CategoryId,Name,Reference,Description,Price,Unit")] Product product,
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
        /// Exibe o formulário para editar um produto existente.
        /// </summary>
        /// <param name="id">O ID do produto a ser editado.</param>
        /// <returns>A view do formulário de edição ou NotFound se não encontrado.</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Items
                .OfType<Product>()
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

        /// <summary>
        /// Atualiza um produto existente no sistema.
        /// </summary>
        /// <param name="id">O ID do produto a ser atualizado.</param>
        /// <param name="product">Os novos dados do produto.</param>
        /// <param name="Images">Lista de novas imagens do produto.</param>
        /// <param name="Tags">Lista de IDs das novas tags do produto.</param>
        /// <param name="Materials">Lista de IDs dos novos materiais usados no produto.</param>
        /// <param name="MaterialsQty">Lista de quantidades dos novos materiais usados.</param>
        /// <returns>Redireciona para a lista de produtos se bem-sucedido, ou retorna a view com erros se falhar.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,UserId,CategoryId,Name,Reference,Description,Price,Unit")] Product product,
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

        /// <summary>
        /// Remove um produto do sistema.
        /// </summary>
        /// <param name="id">O ID do produto a ser removido.</param>
        /// <returns>Redireciona para a lista de produtos após a remoção.</returns>
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

            _context.Items.Remove(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Produto removido com sucesso!";
            return RedirectToAction("Index");

            //return View(product);
        }

        /// <summary>
        /// Verifica se um produto existe no sistema.
        /// </summary>
        /// <param name="id">O ID do produto a ser verificado.</param>
        /// <returns>True se o produto existe, false caso contrário.</returns>
        private bool ProductExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
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
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Produto removido com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "Erro ao remover produto!";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}

