using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Data;

namespace ESA_Terra_Argila.Pages 
{
    [Authorize(Roles = "Admin")] 
    public class LogsModel : PageModel
    {
        private readonly ApplicationDbContext _dbContext;

        public LogsModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<LogEntry> LogEntries { get; set; } = default!;

        public void OnGet()
        {
            LogEntries = _dbContext.LogEntries.OrderByDescending(l => l.Timestamp).ToList();
        }
    }
}
