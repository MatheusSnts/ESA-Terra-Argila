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
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace ESA_Terra_Argila.Controllers
{
    /// <summary>
    /// Controlador responsável por gerenciar as categorias no sistema.
    /// Permite criar, editar, excluir e visualizar categorias que são usadas para classificar materiais e produtos.
    /// </summary>
    [Authorize] // Exige autenticação para todas as ações
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private string? userId;

        /// <summary>
        /// Inicializa uma nova instância do controlador de categorias.
        /// </summary>
        /// <param name="context">O contexto do banco de dados da aplicação.</param>
        /// <param name="userManager">O gerenciador de usuários do Identity.</param>
        public CategoriesController(ApplicationDbContext context, UserManager<User> userManager)
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
        /// Exibe a lista de categorias do usuário atual.
        /// </summary>
        /// <returns>Uma view contendo a lista de categorias.</returns>
        public async Task<IActionResult> Index()
        {
            var categories = _context.Categories.Where(c => c.UserId == userId).Include(c => c.User);
            return View(await categories.ToListAsync());
        }

        /// <summary>
        /// Exibe os detalhes de uma categoria específica.
        /// </summary>
        /// <param name="id">O ID da categoria a ser exibida.</param>
        /// <returns>A view com os detalhes da categoria ou NotFound se não encontrada.</returns>
        [AllowAnonymous] // Permite que qualquer pessoa veja detalhes da categoria
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Categoria não encontrada!";
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Categoria não encontrada!";
                return NotFound();
            }

            return View(category);
        }

        /// <summary>
        /// Exibe o formulário para criar uma nova categoria.
        /// </summary>
        /// <returns>A view do formulário de criação.</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Cria uma nova categoria no sistema.
        /// </summary>
        /// <param name="category">Os dados da categoria a ser criada.</param>
        /// <returns>Redireciona para a lista de categorias se bem-sucedido, ou retorna a view com erros se falhar.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Reference,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.UserId = userId;
                category.CreatedAt = DateTime.UtcNow;
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Categoria criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Erro ao criar categoria! Verifique os campos.";
            return View(category);
        }

        /// <summary>
        /// Exibe o formulário para editar uma categoria existente.
        /// </summary>
        /// <param name="id">O ID da categoria a ser editada.</param>
        /// <returns>A view do formulário de edição ou NotFound se não encontrada.</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Categoria não encontrada!";
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Categoria não encontrada!";
                return NotFound();
            }

            return View(category);
        }

        /// <summary>
        /// Atualiza uma categoria existente no sistema.
        /// </summary>
        /// <param name="id">O ID da categoria a ser atualizada.</param>
        /// <param name="category">Os novos dados da categoria.</param>
        /// <returns>Redireciona para a lista de categorias se bem-sucedido, ou retorna a view com erros se falhar.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Reference,Name")] Category category)
        {
            if (id != category.Id)
            {
                TempData["ErrorMessage"] = "Categoria não encontrada!";
                return NotFound();
            }

            var foundCategory = await _context.Categories.FindAsync(id);

            if (ModelState.IsValid)
            {
                try
                {
                    foundCategory.Name = category.Name;
                    foundCategory.Reference = category.Reference;
                    _context.Update(foundCategory);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Categoria atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        TempData["ErrorMessage"] = "Erro: A categoria não existe mais no sistema!";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Erro ao atualizar categoria! Tente novamente.";
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Erro ao editar categoria! Verifique os campos.";
            return View(category);
        }

        /// <summary>
        /// Remove uma categoria do sistema.
        /// </summary>
        /// <param name="id">O ID da categoria a ser removida.</param>
        /// <returns>Redireciona para a lista de categorias após a remoção.</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Categoria não encontrada!";
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Categoria não encontrada!";
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Categoria removida com sucesso!";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Confirma a remoção de uma categoria do sistema.
        /// </summary>
        /// <param name="id">O ID da categoria a ser removida.</param>
        /// <returns>Redireciona para a lista de categorias após a remoção.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Categoria removida com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "Erro ao remover categoria!";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Verifica se uma categoria existe no sistema.
        /// </summary>
        /// <param name="id">O ID da categoria a ser verificada.</param>
        /// <returns>True se a categoria existe, false caso contrário.</returns>
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
