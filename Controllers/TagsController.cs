using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;

namespace ESA_Terra_Argila.Controllers
{
    public class TagsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string? userId;

        public TagsController(ApplicationDbContext context)
        {
            _context = context;
            userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        // GET: Tags
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tags.Where(t => t.UserId == userId).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Tags/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        // GET: Tags/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Tags/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Reference,UserId,Name,IsPublic,CreatedAt")] Tag tag)
        {
            if (ModelState.IsValid)
            {
                tag.CreatedAt = DateTime.UtcNow;
                tag.UserId = userId;
                _context.Add(tag);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tag adicionada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Erro ao adicionar tag!";
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", tag.UserId);
            return View(tag);
        }

        // GET: Tags/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", tag.UserId);
            return View(tag);
        }

        // POST: Tags/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Reference,UserId,Name,IsPublic,CreatedAt")] Tag tag)
        {
            if (id != tag.Id)
            {
                return NotFound();
            }

            var foundTag = await _context.Tags.FindAsync(id);

            if (ModelState.IsValid)
            {
                try
                {
                    foundTag.Name = tag.Name;
                    foundTag.Reference = tag.Reference;

                    _context.Update(foundTag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(tag.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", tag.UserId);
            return View(tag);
        }

        // GET: Tags/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tag removida com sucesso!";
            return RedirectToAction("Index");

            return View(tag);
        }

        // POST: Tags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag != null)
            {
                _context.Tags.Remove(tag);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}
