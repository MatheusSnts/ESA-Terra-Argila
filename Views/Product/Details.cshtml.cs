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
    public class DetailsModel : PageModel
    {
        private readonly ESA_Terra_Argila.Data.ApplicationDbContext _context;

        public DetailsModel(ESA_Terra_Argila.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public ESA_Terra_Argila.Models.Product Product { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            else
            {
                Product = product;
            }
            return Page();
        }
    }
}
