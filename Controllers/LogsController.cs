using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ESA_Terra_Argila.Controllers
{
    [Authorize(Roles = "Admin")] // Restrito a administradores
    public class LogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listar todos os logs com filtros
        public async Task<IActionResult> Index( string searchIP, string searchPath)
        {
            var logs = _context.AccessLogs.AsQueryable();

          
            if (!string.IsNullOrEmpty(searchIP))
            {
                logs = logs.Where(log => log.IP.Contains(searchIP));
            }

            if (!string.IsNullOrEmpty(searchPath))
            {
                logs = logs.Where(log => log.Path.Contains(searchPath));
            }

            // Ordenar por data mais recente
            var logList = await logs.OrderByDescending(log => log.Timestamp).ToListAsync();

            return View(logList);
        }
    }
}
