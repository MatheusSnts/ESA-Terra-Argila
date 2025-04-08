using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Authorization;
using ESA_Terra_Argila.Helpers;
using NuGet.Packaging;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Identity;
using ESA_Terra_Argila.Areas.Identity.Pages.Account.Manage;
using ESA_Terra_Argila.Enums;

namespace ESA_Terra_Argila.Controllers
{
    /// <summary>
    /// Controlador responsável pelo gerenciamento de materiais no sistema.
    /// Gerencia a listagem, criação, edição, exclusão e controle de estoque de materiais.
    /// </summary>
    [Authorize(Policy = "AcceptedByAdmin")]
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private string? userId;
        private readonly UserManager<User> _userManager;
        private readonly EmailModel? _emailModel;
        private readonly ILogger<MaterialsController> _logger;


        /// <summary>
        /// Inicializa uma nova instância do controlador de materiais.
        /// </summary>
        /// <param name="context">Contexto do banco de dados da aplicação.</param>
        /// <param name="userManager">Gerenciador de usuários do sistema.</param>
        /// <param name="logger">Serviço de log da aplicação.</param>
        public MaterialsController(ApplicationDbContext context, UserManager<User> userManager, ILogger<MaterialsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;

        }

        /// <summary>
        /// Método executado antes de cada ação do controlador para obter o ID do usuário atual.
        /// </summary>
        /// <param name="context">Contexto da execução da ação.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            userId = _userManager.GetUserId(User);
        }

        /// <summary>
        /// Exibe a página principal de materiais. Para fornecedores, mostra materiais favoritos;
        /// para outros usuários, mostra materiais próprios.
        /// </summary>
        /// <returns>A visão de índice com a lista de materiais apropriada para o tipo de usuário.</returns>
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(user);
            bool isVendor = userRoles.Contains("Vendor");

            IQueryable<Material> materials;

            if (isVendor)
            {
                materials = _context.UserMaterialFavorites
                    .Where(umf => umf.UserId == userId && umf.Material.DeletedAt == null && umf.Material.User.DeletedAt == null)
                    .Include(m => m.Material.Category)
                    .Include(m => m.User)
                    .Select(umf => umf.Material);

                return View("VendorIndex", await materials.ToListAsync());
            }
            else
            {
                materials = _context.Items
                    .OfType<Material>()
                    .Where(m => m.UserId == userId && m.DeletedAt == null && m.User.DeletedAt == null)
                    .Include(m => m.Category)
                    .Include(m => m.User);
                return View(await materials.ToListAsync());
            }
        }

        /// <summary>
        /// Exibe a listagem pública de materiais com opções de filtro e ordenação.
        /// </summary>
        /// <param name="page">Número da página atual.</param>
        /// <param name="orderBy">Critério de ordenação (asc/desc por preço).</param>
        /// <param name="priceMin">Preço mínimo para filtro.</param>
        /// <param name="priceMax">Preço máximo para filtro.</param>
        /// <param name="suppliers">Lista de IDs de fornecedores para filtro.</param>
        /// <returns>A visão de lista com materiais paginados e filtrados.</returns>
        [AllowAnonymous]
        public async Task<IActionResult> List(int? page, string? orderBy, float? priceMin, float? priceMax, List<string>? suppliers)
        {
            var query = _context.Items
                .OfType<Material>()
                .Where(m => m.DeletedAt == null && m.User.DeletedAt == null)
                .Include(m => m.Category)
                .Include(m => m.User)
                .Include(m => m.Images)
                .AsQueryable();

            if (priceMin.HasValue)
            {
                query = query.Where(m => m.Price >= priceMin.Value);
            }
            if (priceMax.HasValue)
            {
                query = query.Where(m => m.Price <= priceMax.Value);
            }

            if (suppliers != null && suppliers.Any())
            {
                query = query.Where(m => suppliers.Contains(m.UserId));
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
            var materialsPage = query.ToPagedList(pageNumber, 30);

            if (userId != null)
            {
                var idsOnPage = materialsPage.Select(m => m.Id).ToList();

                var favoriteMaterialIds = await _context.UserMaterialFavorites
                    .Where(f => f.UserId == userId && idsOnPage.Contains(f.MaterialId))
                    .Select(f => f.MaterialId)
                    .ToListAsync();

                ViewData["FavoriteMaterials"] = favoriteMaterialIds;
            }
            else
            {
                ViewData["FavoriteMaterials"] = new List<int>();
            }


            ViewBag.MaterialsPage = materialsPage;

            var users = await _context.Users.ToListAsync();
            var suppliersList = new List<User>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Supplier"))
                {
                    suppliersList.Add(user);
                }
            }

            ViewData["Suppliers"] = new SelectList(suppliersList, "Id", "FullName", suppliers);
            ViewData["SelectedSuppliers"] = suppliers;
            return View();
        }

        /// <summary>
        /// Marca ou desmarca um material como favorito para o usuário atual.
        /// </summary>
        /// <param name="request">Modelo com ID do material e status de favorito.</param>
        /// <returns>Resultado JSON com status da operação.</returns>
        [HttpPost]
        public async Task<IActionResult> SetFavorite([FromBody] FavoriteRequestModel request)
        {
            if (request == null || request.Id <= 0)
            {
                return BadRequest("Dados inválidos.");
            }

            var existingFavorite = await _context.UserMaterialFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.MaterialId == request.Id);

            if (request.IsFavorite)
            {
                if (existingFavorite == null)
                {
                    var favorite = new UserMaterialFavorite
                    {
                        UserId = userId,
                        MaterialId = request.Id
                    };
                    _context.UserMaterialFavorites.Add(favorite);
                }
            }
            else
            {
                if (existingFavorite != null)
                {
                    _context.UserMaterialFavorites.Remove(existingFavorite);
                }
            }

            await _context.SaveChangesAsync();


            return Ok(new { success = true, message = "Favorito atualizado!" });
        }

        /// <summary>
        /// Remove um material da lista de favoritos do usuário atual.
        /// </summary>
        /// <param name="id">ID do material a ser removido dos favoritos.</param>
        /// <returns>Redirecionamento para a página de índice.</returns>
        [HttpPost]
        public async Task<IActionResult> UnsetFavorite(int id)
        {

            var favorite = await _context.UserMaterialFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.MaterialId == id);

            if (favorite != null)
            {
                _context.UserMaterialFavorites.Remove(favorite);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "O material foi removido dos favoritos com sucesso!";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Exibe os detalhes de um material específico.
        /// </summary>
        /// <param name="id">ID do material a ser visualizado.</param>
        /// <returns>A visão de detalhes do material.</returns>
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            var material = await _context.Items
                .OfType<Material>()
                .Where(m => m.DeletedAt == null && m.User.DeletedAt == null)
                .Include(m => m.Category)
                .Include(m => m.User)
                .Include(m => m.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            bool isFavorite = await _context.UserMaterialFavorites
                .AnyAsync(f => f.UserId == userId && f.MaterialId == material.Id);

            ViewData["IsFavorite"] = isFavorite;

            return View(material);
        }

        /// <summary>
        /// Exibe o formulário para criação de um novo material.
        /// </summary>
        /// <returns>A visão do formulário de criação.</returns>
        public IActionResult Create()
        {
            ViewData["Categories"] = new SelectList(_context.Categories.Where(c => c.UserId == userId), "Id", "Name");
            ViewData["Tags"] = new SelectList(_context.Tags.Where(t => t.UserId == userId), "Id", "Name");
            ViewData["Units"] = UnitsHelper.GetUnitsSelectList();
            return View();
        }

        /// <summary>
        /// Processa a submissão do formulário para criação de um novo material.
        /// </summary>
        /// <param name="material">Dados do material a ser criado.</param>
        /// <param name="Images">Lista de arquivos de imagem para o material.</param>
        /// <param name="Tags">Lista de IDs de tags associadas ao material.</param>
        /// <returns>Redirecionamento para a página de índice em caso de sucesso ou exibição do formulário com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Name,Reference,Description,Price,Unit,IsSustainable")] Material material, List<IFormFile> Images, List<int> Tags)
        {
            if (ModelState.IsValid)
            {
                material.UserId = userId;
                material.CreatedAt = DateTime.UtcNow;
                if (Tags != null && Tags.Any())
                {
                    var selectedTags = await _context.Tags.Where(t => Tags.Contains(t.Id)).ToListAsync();
                    material.Tags = selectedTags;
                }

                _context.Add(material);
                await _context.SaveChangesAsync();

                if (Images != null && Images.Count > 0)
                {
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{ImageHelper.ItemImagesFolder}{material.Id}");

                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    foreach (var file in Images)
                    {
                        if (file.Length > 0)
                        {
                            ItemImage productImage = await ImageHelper.SaveItemImage(file, material.Id, imagesFolder);

                            _context.ItemImages.Add(productImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                TempData["SuccessMessage"] = "Material adicionado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Erro ao adicionar material! Verifique os campos.";
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.UserId == userId), "Id", "Id", material.CategoryId);
            return View(material);
        }

        /// <summary>
        /// Exibe o formulário para edição de um material existente.
        /// </summary>
        /// <param name="id">ID do material a ser editado.</param>
        /// <returns>A visão do formulário de edição preenchido com os dados do material.</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            var material = await _context.Items
                .OfType<Material>()
                .Where(m => m.DeletedAt == null && m.User.DeletedAt == null)
                .Include(m => m.Tags)
                .Include(m => m.Images)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            var allTags = _context.Tags.Where(t => t.UserId == userId);
            var selectedTagIds = material.Tags.Select(t => t.Id).ToList();
            ViewData["Categories"] = new SelectList(_context.Categories.Where(c => c.UserId == userId), "Id", "Name");
            ViewData["Tags"] = new SelectList(allTags, "Id", "Name", selectedTagIds);
            ViewData["Units"] = UnitsHelper.GetUnitsSelectList();
            return View(material);
        }

        /// <summary>
        /// Processa a submissão do formulário para edição de um material existente.
        /// </summary>
        /// <param name="id">ID do material a ser editado.</param>
        /// <param name="material">Dados atualizados do material.</param>
        /// <param name="Images">Lista de novos arquivos de imagem para o material.</param>
        /// <param name="Tags">Lista atualizada de IDs de tags associadas ao material.</param>
        /// <returns>Redirecionamento para a página de índice em caso de sucesso ou exibição do formulário com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryId,Name,Reference,Description,Price,Unit,IsSustainable")] Material material, List<IFormFile> Images, List<int> Tags)
        {
            if (id != material.Id)
            {
                return NotFound();
            }

            var foundMaterial = await _context.Items
                .OfType<Material>()
                .Where(m => m.DeletedAt == null && m.User.DeletedAt == null)
                .Include(m => m.Tags)
                .Include(m => m.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ModelState.IsValid)
            {
                try
                {
                    foundMaterial.Name = material.Name;
                    foundMaterial.CategoryId = material.CategoryId;
                    foundMaterial.Reference = material.Reference;
                    foundMaterial.Description = material.Description;
                    foundMaterial.Price = material.Price;
                    foundMaterial.Unit = material.Unit;
                    foundMaterial.IsSustainable = material.IsSustainable;

                    if (Tags != null && Tags.Any())
                    {
                        var selectedTags = await _context.Tags.Where(t => Tags.Contains(t.Id)).ToListAsync();
                        foundMaterial.Tags.Clear();
                        foundMaterial.Tags.AddRange(selectedTags);
                    }

                    if (Images != null && Images.Count > 0)
                    {
                        var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{ImageHelper.ItemImagesFolder}{foundMaterial.Id}");

                        foreach (var oldImage in foundMaterial.Images.ToList())
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImage.Path.TrimStart('/'));

                            if (System.IO.File.Exists(oldImagePath))
                            {
                                try
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                                catch (Exception ex)
                                {
                                    return Json(ex.Message);
                                }
                            }
                            else
                            {
                                return Json($"Arquivo não encontrado: {oldImagePath}");
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
                                var materialImage = await ImageHelper.SaveItemImage(file, foundMaterial.Id, imagesFolder);

                                _context.ItemImages.Add(materialImage);
                            }
                        }
                    }

                    _context.Update(foundMaterial);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Material atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(material.Id))
                    {
                        TempData["ErrorMessage"] = "Erro: O material não existe mais no sistema!";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Erro ao atualizar material! Tente novamente.";
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Erro ao editar material! Verifique os campos.";
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", material.CategoryId);
            return View(material);
        }

        /// <summary>
        /// Exibe a página de confirmação para exclusão de um material.
        /// </summary>
        /// <param name="id">ID do material a ser excluído.</param>
        /// <returns>Redirecionamento para a página de índice após a exclusão.</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            var material = await _context.Items
                .OfType<Material>()
                .Include(m => m.Category)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            material.DeletedAt = DateTime.UtcNow;
            _context.Items.Update(material);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Material removido com sucesso!";
            return RedirectToAction("Index");

            //return View(material);
        }

        /// <summary>
        /// Processa a confirmação de exclusão de um material.
        /// </summary>
        /// <param name="id">ID do material a ser excluído.</param>
        /// <returns>Redirecionamento para a página de índice após a exclusão.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Items.FindAsync(id);
            if (material != null)
            {
                _context.Items.Remove(material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Material removido com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "Erro ao remover material!";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Verifica se um material com o ID especificado existe no banco de dados.
        /// </summary>
        /// <param name="id">ID do material a verificar.</param>
        /// <returns>Verdadeiro se o material existir, falso caso contrário.</returns>
        private bool MaterialExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
        
        /// <summary>
        /// Exibe o histórico de movimentações de estoque de um material.
        /// </summary>
        /// <param name="id">ID do material a ser consultado.</param>
        /// <returns>A visão com o histórico de movimentações de estoque.</returns>
        public async Task<IActionResult> StockHistory(int id)
        {
            var material = await _context.Items
                .OfType<Material>()
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            var movements = await _context.StockMovements
                .Where(m => m.MaterialId == id)
                .OrderByDescending(m => m.Date)
                .Include(m => m.User)
                .ToListAsync();

            ViewData["Material"] = material;
            return View(movements);
        }

        /// <summary>
        /// Exibe o formulário para criação de uma nova movimentação de estoque.
        /// </summary>
        /// <param name="id">ID do material para registrar a movimentação.</param>
        /// <returns>A visão do formulário de criação de movimentação de estoque.</returns>
        public IActionResult CreateStockMovement(int id)
        {
            var material = _context.Items.Find(id);
            if (material == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return RedirectToAction(nameof(Index));
            }

            var movement = new StockMovement
            {
                MaterialId = material.Id
            };

            return View(movement);
        }

        /// <summary>
        /// Processa a submissão do formulário para criação de uma nova movimentação de estoque.
        /// </summary>
        /// <param name="movement">Dados da movimentação de estoque a ser registrada.</param>
        /// <returns>Redirecionamento para a página de índice em caso de sucesso.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStockMovement([Bind("MaterialId,Quantity,Type")] StockMovement movement)
        {
            _logger.LogInformation("Entrou no método CreateStockMovement");

            var material = await _context.Items.FindAsync(movement.MaterialId);
            if (material == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return RedirectToAction(nameof(Index));
            }

            // Atualizar o stock
            if (movement.Type == "Entrada")
            {
                material.Stock += movement.Quantity;
            }
            else if (movement.Type == "Saída")
            {
                if (material.Stock < movement.Quantity)
                {
                    TempData["ErrorMessage"] = "Estoque insuficiente!";
                    return View(movement);
                }
                material.Stock -= movement.Quantity;
            }

            // Guardar a movimentação no histórico
            movement.Date = DateTime.UtcNow;
            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            // Registrar log do movimento
            _logger.LogInformation($"Movimento registado: {movement.Type} de {movement.Quantity} para o material {material.Name}.");

            TempData["SuccessMessage"] = "Movimentação de estoque registada!";
            return RedirectToAction("Index", "Materials");
        }

        /// <summary>
        /// Atualiza o estoque de um material diretamente com um novo valor.
        /// </summary>
        /// <param name="id">ID do material a ter o estoque atualizado.</param>
        /// <param name="novoStock">Novo valor de estoque a ser definido.</param>
        /// <returns>Resultado JSON com mensagem de confirmação.</returns>
        public async Task<IActionResult> AtualizarStock(int id, int novoStock)
        {
            var material = await _context.Items.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            material.Stock = novoStock;
            _context.Update(material);
            await _context.SaveChangesAsync();

            // Chama o método de envio de e-mail se o stock for 0
            if (material.Stock == 0)
            {
                await _emailModel.SendStockAlertEmailAsync(material.Name);
            }

            return Ok(new { message = "Stock atualizado." });
        }

        

    }

    /// <summary>
    /// Modelo que representa uma solicitação para marcar/desmarcar um material como favorito.
    /// </summary>
    public class FavoriteRequestModel
    {
        /// <summary>
        /// ID do material a ser marcado/desmarcado como favorito.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Indica se o material deve ser marcado como favorito (true) ou desmarcado (false).
        /// </summary>
        public bool IsFavorite { get; set; }
    }
}
