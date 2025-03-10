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

namespace ESA_Terra_Argila.Controllers
{
    [Authorize]
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string? userId;


        public MaterialsController(ApplicationDbContext context)
        {
            _context = context;
            userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        // GET: Materials
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Materials.Where(m => m.UserId == userId).Include(m => m.Category).Include(m => m.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Materials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            var material = await _context.Materials
                .Include(m => m.Category)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            return View(material);
        }

        // GET: Materials/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id");
            return View();
        }

        // POST: Materials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Name,Reference,Description,Price,Unit")] Material material)
        {
            if (ModelState.IsValid)
            {
                material.UserId = userId;
                material.CreatedAt = DateTime.UtcNow;

                _context.Add(material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Material adicionado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Erro ao adicionar material! Verifique os campos.";
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", material.CategoryId);
            return View(material);
        }

        // GET: Materials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", material.CategoryId);
            return View(material);
        }

        // POST: Materials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryId,Name,Reference,Description,Price,Unit")] Material material)
        {
            if (id != material.Id)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            var foundMaterial = await _context.Materials.FindAsync(id);

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

        // GET: Materials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            var material = await _context.Materials
                .Include(m => m.Category)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                TempData["ErrorMessage"] = "Material não encontrado!";
                return NotFound();
            }

            return View(material);
        }

        // POST: Materials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material != null)
            {
                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Material removido com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "Erro ao remover material!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MaterialExists(int id)
        {
            return _context.Materials.Any(e => e.Id == id);
        }
    }
}
