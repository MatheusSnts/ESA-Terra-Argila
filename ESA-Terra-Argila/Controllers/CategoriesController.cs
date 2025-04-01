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
    /// Controller responsável pelo gerenciamento de categorias.
    /// Todas as ações requerem autenticação, exceto quando explicitamente permitido.
    /// </summary>
    [Authorize] // Exige autenticação para todas as ações
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private string? userId;


        /// <summary>
        /// Construtor do CategoriesController.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        /// <param name="userManager">Gerenciador de usuários</param>
        public CategoriesController(ApplicationDbContext context, UserManager<User> userManager)
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
        /// Exibe a lista de categorias do usuário atual.
        /// </summary>
        /// <returns>View com a lista de categorias</returns>
        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = _context.Categories.Where(c => c.UserId == userId).Include(c => c.User);
            return View(await categories.ToListAsync());
        }

        /// <summary>
        /// Exibe os detalhes de uma categoria específica.
        /// Ação acessível por qualquer usuário, mesmo não autenticado.
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <returns>View com os detalhes da categoria ou NotFound se não existir</returns>
        // GET: Categories/Details/5
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
        /// Exibe o formulário para criação de uma nova categoria.
        /// </summary>
        /// <returns>View com o formulário de criação</returns>
        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Processa o envio do formulário de criação de uma nova categoria.
        /// </summary>
        /// <param name="category">Dados da categoria a ser criada</param>
        /// <returns>Redireciona para Index em caso de sucesso ou retorna à View com os dados em caso de erro</returns>
        // POST: Categories/Create
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
        /// Exibe o formulário para edição de uma categoria existente.
        /// </summary>
        /// <param name="id">ID da categoria a ser editada</param>
        /// <returns>View com o formulário de edição ou NotFound se a categoria não existir</returns>
        // GET: Categories/Edit/5
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
        /// Processa o envio do formulário de edição de uma categoria.
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <param name="category">Dados atualizados da categoria</param>
        /// <returns>Redireciona para Index em caso de sucesso ou retorna à View com os dados em caso de erro</returns>
        // POST: Categories/Edit/5
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
        /// Remove uma categoria específica. Esta ação remove diretamente a categoria sem confirmação.
        /// </summary>
        /// <param name="id">ID da categoria a ser removida</param>
        /// <returns>Redireciona para Index após a remoção ou NotFound se a categoria não existir</returns>
        // GET: Categories/Delete/5
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

            //return View(category);
        }

        /// <summary>
        /// Processa a confirmação de remoção de uma categoria. (Método não utilizado atualmente)
        /// </summary>
        /// <param name="id">ID da categoria a ser removida</param>
        /// <returns>Redireciona para Index após a remoção</returns>
        // POST: Categories/Delete/5
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
        /// Verifica se uma categoria existe no banco de dados.
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <returns>True se a categoria existir, False caso contrário</returns>
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
