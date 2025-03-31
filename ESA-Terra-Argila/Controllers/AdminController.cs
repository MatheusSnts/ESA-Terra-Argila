using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Helpers;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESA_Terra_Argila.Controllers
{
    /// <summary>
    /// Controlador responsável por gerenciar as operações administrativas do sistema.
    /// Permite gerenciar usuários, aprovar novos usuários e gerenciar diferentes tipos de usuários (vendedores, fornecedores, clientes).
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Inicializa uma nova instância do controlador administrativo.
        /// </summary>
        /// <param name="context">O contexto do banco de dados da aplicação.</param>
        /// <param name="userManager">O gerenciador de usuários do Identity.</param>
        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Exibe a lista de usuários pendentes de aprovação.
        /// </summary>
        /// <returns>A view com a lista de usuários que precisam ser aprovados.</returns>
        public async Task<IActionResult> AcceptUsers()
        {
            var users = _context.Users.Where(u => !u.AcceptedByAdmin);
            var usersList = await users.ToListAsync();
            foreach (var user in usersList) 
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.Role = UserRoleHelper.GetUserRoleFromString(roles.FirstOrDefault());
            }
            return View(usersList);
        }

        /// <summary>
        /// Exibe a lista de vendedores aprovados no sistema.
        /// </summary>
        /// <returns>A view com a lista de vendedores.</returns>
        public async Task<IActionResult> Vendors()
        {
            return View(await GetUsersByRole("Vendor"));
        }

        /// <summary>
        /// Exibe a lista de fornecedores aprovados no sistema.
        /// </summary>
        /// <returns>A view com a lista de fornecedores.</returns>
        public async Task<IActionResult> Suppliers()
        {
            return View(await GetUsersByRole("Supplier"));
        }

        /// <summary>
        /// Exibe a lista de clientes aprovados no sistema.
        /// </summary>
        /// <returns>A view com a lista de clientes.</returns>
        public async Task<IActionResult> Customers()
        {
            return View(await GetUsersByRole("Customer"));
        }

        /// <summary>
        /// Aprova um usuário no sistema.
        /// </summary>
        /// <param name="id">O ID do usuário a ser aprovado.</param>
        /// <returns>Redireciona para a lista de usuários pendentes após a aprovação.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            user.AcceptedByAdmin = true;
            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador aprovado com sucesso!";
            return RedirectToAction(nameof(AcceptUsers));
        }

        /// <summary>
        /// Remove um usuário do sistema.
        /// </summary>
        /// <param name="id">O ID do usuário a ser removido.</param>
        /// <returns>Redireciona para a lista de usuários pendentes após a remoção.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador removido com sucesso";
            return RedirectToAction(nameof(AcceptUsers));
        }

        /// <summary>
        /// Bloqueia um usuário no sistema.
        /// </summary>
        /// <param name="id">O ID do usuário a ser bloqueado.</param>
        /// <returns>Redireciona para a página anterior após o bloqueio.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            user.AcceptedByAdmin = false;
            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador bloqueado com sucesso!";
            var referer = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(AcceptUsers));
        }

        /// <summary>
        /// Envia um convite para um usuário se tornar vendedor.
        /// </summary>
        /// <param name="id">O ID do usuário a ser convidado.</param>
        /// <returns>Redireciona para a lista de usuários pendentes.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteVendor(string id)
        {
            // TODO: not implemented yet
            return RedirectToAction(nameof(AcceptUsers));
        }

        /// <summary>
        /// Envia um convite para um usuário se tornar fornecedor.
        /// </summary>
        /// <param name="id">O ID do usuário a ser convidado.</param>
        /// <returns>Redireciona para a lista de usuários pendentes.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteSupplier(string id)
        {
            // TODO: not implemented yet
            return RedirectToAction(nameof(AcceptUsers));
        }

        /// <summary>
        /// Obtém a lista de usuários por função específica.
        /// </summary>
        /// <param name="role">A função dos usuários a serem retornados.</param>
        /// <returns>Lista de usuários com a função especificada.</returns>
        private async Task<List<User>> GetUsersByRole(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            return users.Where(u => u.AcceptedByAdmin).ToList();
        }
    }
}
