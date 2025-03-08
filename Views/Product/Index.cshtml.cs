using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;

namespace ESA_Terra_Argila.Views.Product
{
    public class IndexModel : PageModel
    {
        private readonly ESA_Terra_Argila.Data.ApplicationDbContext _context;

        public IndexModel(ESA_Terra_Argila.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ESA_Terra_Argila.Models.Product> Product { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User).ToListAsync();
        }
    }
}
