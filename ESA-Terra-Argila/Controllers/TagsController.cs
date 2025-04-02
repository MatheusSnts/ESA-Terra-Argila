using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace ESA_Terra_Argila.Controllers
{
    /// <summary>
    /// Controller responsável pelo gerenciamento de tags no sistema.
    /// Permite a criação, edição, visualização e remoção de tags associadas a produtos e materiais.
    /// </summary>
    public class TagsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private string? userId;

        /// <summary>
        /// Inicializa uma nova instância do controller de tags.
        /// </summary>
        /// <param name="context">Contexto do banco de dados da aplicação.</param>
        /// <param name="userManager">Gerenciador de usuários do Identity.</param>
        public TagsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Método executado antes de cada ação do controller para obter o ID do usuário atual.
        /// </summary>
        /// <param name="context">Contexto da execução da ação.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            userId = _userManager.GetUserId(User);
        }

        /// <summary>
        /// Exibe a lista de tags do usuário atual.
        /// </summary>
        /// <returns>View com a lista de tags do usuário.</returns>
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tags.Where(t => t.UserId == userId).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        /// <summary>
        /// Exibe os detalhes de uma tag específica.
        /// </summary>
        /// <param name="id">ID da tag a ser visualizada.</param>
        /// <returns>View com os detalhes da tag ou NotFound se não existir.</returns>
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

        /// <summary>
        /// Exibe o formulário para criação de uma nova tag.
        /// </summary>
        /// <returns>View com o formulário de criação.</returns>
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        /// <summary>
        /// Processa o envio do formulário de criação de uma nova tag.
        /// </summary>
        /// <param name="tag">Dados da tag a ser criada.</param>
        /// <returns>Redirecionamento para Index em caso de sucesso ou View com erros em caso de falha.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Reference,UserId,Name,IsPublic")] Tag tag)
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
            return Json(ModelState.Values);
            TempData["ErrorMessage"] = "Erro ao adicionar tag!";
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", tag.UserId);
            return View(tag);
        }

        /// <summary>
        /// Exibe o formulário para edição de uma tag existente.
        /// </summary>
        /// <param name="id">ID da tag a ser editada.</param>
        /// <returns>View com o formulário de edição ou NotFound se a tag não existir.</returns>
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

        /// <summary>
        /// Processa o envio do formulário de edição de uma tag.
        /// </summary>
        /// <param name="id">ID da tag a ser editada.</param>
        /// <param name="tag">Dados atualizados da tag.</param>
        /// <returns>Redirecionamento para Index em caso de sucesso ou View com erros em caso de falha.</returns>
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

        /// <summary>
        /// Remove uma tag específica do sistema.
        /// </summary>
        /// <param name="id">ID da tag a ser removida.</param>
        /// <returns>Redirecionamento para Index após a remoção ou NotFound se a tag não existir.</returns>
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

        /// <summary>
        /// Processa a confirmação de remoção de uma tag.
        /// </summary>
        /// <param name="id">ID da tag a ser removida.</param>
        /// <returns>Redirecionamento para Index após a remoção.</returns>
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

        /// <summary>
        /// Verifica se uma tag com o ID especificado existe no banco de dados.
        /// </summary>
        /// <param name="id">ID da tag a verificar.</param>
        /// <returns>Verdadeiro se a tag existir, falso caso contrário.</returns>
        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}
