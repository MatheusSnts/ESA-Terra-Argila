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

namespace ESA_Terra_Argila.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private string? userId;
        private readonly UserManager<User> _userManager;


        public ProductsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            userId = _userManager.GetUserId(User);
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Where(p => p.UserId == userId).Include(p => p.Category).Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> List(int? page, string? orderBy, float? priceMin, float? priceMax, List<string>? vendors)
        {
            var query = _context.Products
                .Include(m => m.Category)
                .Include(m => m.User)
                .Include(m => m.Images)
                .AsQueryable();

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
            return View();
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
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
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{ImageHelper.ProductImagesFolder}{product.Id}");

                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    foreach (var file in Images)
                    {
                        if (file.Length > 0)
                        {
                            ProductImage productImage = await ImageHelper.SaveProductImage(file, product.Id, imagesFolder);

                            _context.ProductImages.Add(productImage);
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

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
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

            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

            var foundProduct = await _context.Products
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
                        var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{ImageHelper.ProductImagesFolder}{foundProduct.Id}");

                        foreach (var oldImage in foundProduct.Images.ToList())
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImage.Path.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            _context.ProductImages.Remove(oldImage);
                        }

                        await _context.SaveChangesAsync();

                        if (!Directory.Exists(imagesFolder))
                            Directory.CreateDirectory(imagesFolder);

                        foreach (var file in Images)
                        {
                            if (file.Length > 0)
                            {
                                var productImage = await ImageHelper.SaveProductImage(file, foundProduct.Id, imagesFolder);
                                _context.ProductImages.Add(productImage);
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

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
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
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
