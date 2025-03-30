using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESA_Terra_Argila.Helpers
{
    public static class UnitsHelper
    {
        private static readonly Dictionary<string, string> units = new()
        {
            { "un", "Unidade" },
            { "kg", "Kg" },
            { "g", "Gramas" },
            { "l", "Litros" },
            { "ml", "Mililitros" },
            { "m", "Metros" },
            { "cm", "Centímetros" }
        };

        public static SelectList GetUnitsSelectList()
        {
            var list = units.Select(u => new SelectListItem
            {
                Value = u.Key,
                Text = u.Value
            }).ToList();

            return new SelectList(list, "Value", "Text");
        }

        public static string GetUnitName(string unit)
        {
            return units[unit];
        }
    }

}
