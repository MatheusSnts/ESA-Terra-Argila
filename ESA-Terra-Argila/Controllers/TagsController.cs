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
    /// Controlador responsável por gerenciar as tags no sistema.
    /// Permite criar, editar, excluir e visualizar tags que são usadas para classificar materiais e produtos.
    /// </summary>
    public class TagsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private string? userId;

        /// <summary>
        /// Inicializa uma nova instância do controlador de tags.
        /// </summary>
        /// <param name="context">O contexto do banco de dados da aplicação.</param>
        /// <param name="userManager">O gerenciador de usuários do Identity.</param>
        public TagsController(ApplicationDbContext context, UserManager<User> userManager)
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
        /// Exibe a lista de tags do usuário atual.
        /// </summary>
        /// <returns>Uma view contendo a lista de tags.</returns>
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tags.Where(t => t.UserId == userId).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        /// <summary>
        /// Exibe os detalhes de uma tag específica.
        /// </summary>
        /// <param name="id">O ID da tag a ser exibida.</param>
        /// <returns>A view com os detalhes da tag ou NotFound se não encontrada.</returns>
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
        /// Exibe o formulário para criar uma nova tag.
        /// </summary>
        /// <returns>A view do formulário de criação.</returns>
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        /// <summary>
        /// Cria uma nova tag no sistema.
        /// </summary>
        /// <param name="tag">Os dados da tag a ser criada.</param>
        /// <returns>Redireciona para a lista de tags se bem-sucedido, ou retorna a view com erros se falhar.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Reference,Name")] Tag tag)
        {
            if (ModelState.IsValid)
            {
                tag.UserId = userId;
                tag.CreatedAt = DateTime.UtcNow;
                _context.Add(tag);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tag criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Erro ao criar tag! Verifique os campos.";
            return View(tag);
        }

        /// <summary>
        /// Exibe o formulário para editar uma tag existente.
        /// </summary>
        /// <param name="id">O ID da tag a ser editada.</param>
        /// <returns>A view do formulário de edição ou NotFound se não encontrada.</returns>
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

            return View(tag);
        }

        /// <summary>
        /// Atualiza uma tag existente no sistema.
        /// </summary>
        /// <param name="id">O ID da tag a ser atualizada.</param>
        /// <param name="tag">Os novos dados da tag.</param>
        /// <returns>Redireciona para a lista de tags se bem-sucedido, ou retorna a view com erros se falhar.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Reference,Name")] Tag tag)
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
                    TempData["SuccessMessage"] = "Tag atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(tag.Id))
                    {
                        TempData["ErrorMessage"] = "Erro: A tag não existe mais no sistema!";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Erro ao atualizar tag! Tente novamente.";
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Erro ao editar tag! Verifique os campos.";
            return View(tag);
        }

        /// <summary>
        /// Remove uma tag do sistema.
        /// </summary>
        /// <param name="id">O ID da tag a ser removida.</param>
        /// <returns>Redireciona para a lista de tags após a remoção.</returns>
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
        }

        /// <summary>
        /// Verifica se uma tag existe no sistema.
        /// </summary>
        /// <param name="id">O ID da tag a ser verificada.</param>
        /// <returns>True se a tag existe, false caso contrário.</returns>
        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}
