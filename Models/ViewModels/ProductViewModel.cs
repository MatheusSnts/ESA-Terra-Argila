using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESA_Terra_Argila.Models.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; }
        public IEnumerable<SelectListItem> Materials { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> Tags { get; set; }
    }
}
