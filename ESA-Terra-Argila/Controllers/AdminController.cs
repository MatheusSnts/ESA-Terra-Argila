using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Helpers;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

namespace ESA_Terra_Argila.Controllers
{
    /// <summary>
    /// Controller responsável pelas ações administrativas do sistema.
    /// Requer autenticação e papel de administrador (Admin) para acesso a todas as ações.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        private readonly AdminDashboardService _dashboardService;
        public AdminController(ApplicationDbContext context, UserManager<User> userManager, AdminDashboardService dashboardService)
        {
            _context = context;
            _userManager = userManager;
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Exibe a lista de usuários pendentes de aprovação pelo administrador.
        /// </summary>
        /// <returns>View com a lista de usuários não aprovados</returns>
        public async Task<IActionResult> AcceptUsers()
        {
            var users = _context.Users.Where(u => !u.AcceptedByAdmin && u.DeletedAt == null);
            var usersList = await users.ToListAsync();
            foreach (var user in usersList) 
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.Role = UserRoleHelper.GetUserRoleFromString(roles.FirstOrDefault());
            }
            return View(usersList);
        }


        public async Task<IActionResult> Dashboard()
        {
            var model = await _dashboardService.GetDashboardStatsAsync();
            return View(model);
        }


        public async Task<IActionResult> Vendors()
        {
            return View(await GetUsersByRole("Vendor"));
        }

        /// <summary>
        /// Exibe a lista de fornecedores aprovados no sistema.
        /// </summary>
        /// <returns>View com a lista de fornecedores</returns>
        public async Task<IActionResult> Suppliers()
        {
            return View(await GetUsersByRole("Supplier"));
        }

        /// <summary>
        /// Exibe a lista de clientes aprovados no sistema.
        /// </summary>
        /// <returns>View com a lista de clientes</returns>
        public async Task<IActionResult> Customers()
        {
            return View(await GetUsersByRole("Customer"));
        }

        /// <summary>
        /// Aprova um usuário pendente no sistema.
        /// </summary>
        /// <param name="id">ID do usuário a ser aprovado</param>
        /// <returns>Redireciona para a lista de usuários pendentes</returns>
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
        /// <param name="id">ID do usuário a ser removido</param>
        /// <returns>Redireciona para a lista de usuários pendentes</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            //delete user by id
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador removido com sucesso";
            return RedirectToAction(nameof(AcceptUsers));
        }

        /// <summary>
        /// Bloqueia um usuário aprovado no sistema.
        /// </summary>
        /// <param name="id">ID do usuário a ser bloqueado</param>
        /// <returns>Redireciona para a página anterior ou para a lista de usuários pendentes</returns>
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
        /// Envia um convite para um usuário se juntar como vendedor.
        /// Método não implementado completamente.
        /// </summary>
        /// <param name="id">ID do usuário a ser convidado</param>
        /// <returns>Redireciona para a lista de usuários pendentes</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteVendor(string id)
        {
            // TODO: not implemented yet
            return RedirectToAction(nameof(AcceptUsers));

        }

        /// <summary>
        /// Envia um convite para um usuário se juntar como fornecedor.
        /// Método não implementado completamente.
        /// </summary>
        /// <param name="id">ID do usuário a ser convidado</param>
        /// <returns>Redireciona para a lista de usuários pendentes</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteSupplier(string id)
        {
            // TODO: not implemented yet
            return RedirectToAction(nameof(AcceptUsers));

        }

        /// <summary>
        /// Obtém a lista de usuários com um determinado papel/função.
        /// </summary>
        /// <param name="role">Nome do papel/função</param>
        /// <returns>Lista de usuários com o papel/função especificado</returns>
        private async Task<List<User>> GetUsersByRole(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            return users.Where(u => u.AcceptedByAdmin && u.DeletedAt == null).ToList();
        }



        [HttpGet]
        public async Task<IActionResult> GetActivityData(string range)
        {
            var now = DateTime.UtcNow;
            DateTime start;
            string format;
            Func<DateTime, DateTime> step;

            switch (range)
            {
                case "24h":
                    start = now.AddHours(-23); 
                    format = "HH\\h";
                    break;
                case "7d":
                    start = now.AddDays(-6); 
                    format = "dd/MM";
                    break;
                case "month":
                    start = now.AddMonths(-1);
                    format = "dd/MM";
                    break;
                case "year":
                    start = now.AddYears(-1);
                    format = "MMM yyyy";
                    break;
                case "total":
                    start = now.AddYears(-2); 
                    format = "yyyy";
                    step = dt => dt.AddYears(1);
                    break;
                default:
                    start = DateTime.MinValue;
                    format = "yyyy-MM-dd";
                    break;
            }

            var raw = _context.UserActivities
            .Where(a => a.Timestamp >= start)
            .AsEnumerable()
            .GroupBy(a => a.Timestamp.ToString(format))
            .ToDictionary(g => g.Key, g => g.Count());

            var result = new List<object>();

            if (range == "24h")
            {
                for (int i = 0; i < 24; i++)
                {
                    var label = $"{i:00}h";
                    result.Add(new { label, count = raw.ContainsKey(label) ? raw[label] : 0 });
                }
            }
            else
            {
                var current = start;
                while (current <= now)
                {
                    var label = current.ToString(format);
                    result.Add(new { label, count = raw.ContainsKey(label) ? raw[label] : 0 });
                    current = range switch
                    {
                        "year" => current.AddMonths(1),
                        "month" => current.AddDays(1),
                        _ => current.AddDays(1)

                           
                    };
                 
                }


            }
            
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueData(string range)
        {
            var now = DateTime.UtcNow;
            DateTime start;
            string format;
            TimeSpan step;

            switch (range)
            {
                case "24h":
                    start = now.AddHours(-23);
                    format = "HH\\h";
                    step = TimeSpan.FromHours(1);
                    break;
                case "7d":
                    start = now.AddDays(-6);
                    format = "dd/MM";
                    step = TimeSpan.FromDays(1);
                    break;
                case "month":
                    start = now.AddMonths(-1);
                    format = "dd/MM";
                    step = TimeSpan.FromDays(1);
                    break;
                case "year":
                    start = now.AddYears(-1);
                    format = "MMM yyyy";
                    step = TimeSpan.FromDays(30); 
                    break;
                default:
                    start = DateTime.UtcNow.AddMonths(-1);
                    format = "dd/MM";
                    step = TimeSpan.FromDays(1);
                    break;
            }

            var raw = await _context.Payments
                .Where(p => p.PaymentDateTime >= start)
                .ToListAsync();

            var grouped = raw
                .GroupBy(p => p.PaymentDateTime.ToString(format))
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            var result = new List<object>();
            var current = start;

            while (current <= now)
            {
                var label = current.ToString(format);
                result.Add(new
                {
                    label,
                    total = grouped.ContainsKey(label) ? grouped[label] : 0
                });

                current = range switch
                {
                    "year" => current.AddMonths(1),
                    "month" => current.AddDays(1),
                    "7d" or "total" => current.AddDays(1),
                    "24h" => current.AddHours(1),
                    _ => current.AddDays(1)
                };
            }

            return Json(result);
        }



        private async Task<List<User>> GetDeletedUsers()
        {
            return await _context.Users
                .Where(u => u.DeletedAt != null)
                .ToListAsync();
        }

    }
}
