
using System.ComponentModel.DataAnnotations;

namespace ESA_Terra_Argila.Enums
    {
        // Enum for the type of unit of a product or material (e.g. gram, kilogram, liter, etc.)
        // If more needed, add more values to the enum and update the database 
        public enum UnitType
        {
            [Display(Name = "Unidade")]
            None = 0,

            [Display(Name = "Grama")]
            Gram = 1,

            [Display(Name = "Quilograma")]
            Kilogram = 2,

            [Display(Name = "Litro")]
            Liter = 3,

            [Display(Name = "Mililitro")]
            Milliliter = 4,

            [Display(Name = "Unidade")]
            Piece = 5,

            [Display(Name = "Metro")]
            Meter = 6,

            [Display(Name = "Metro Quadrado")]
            SquareMeter = 7,

            [Display(Name = "Metro Cúbico")]
            CubicMeter = 8,

            [Display(Name = "Tonelada")]
            Ton = 9,

            [Display(Name = "Milímetro")]
            Millimeter = 10,

            [Display(Name = "Centímetro")]
            Centimeter = 11,

            [Display(Name = "Quilômetro")]
            Kilometer = 12,

            [Display(Name = "Polegada")]
            Inch = 13,

            [Display(Name = "Onça")]
            Ounce = 17,

            [Display(Name = "Libra")]
            Pound = 18,

            [Display(Name = "Galão")]
            Gallon = 19,

            [Display(Name = "Miligrama")]
            Milligram = 26,

            [Display(Name = "Centigrama")]
            Centigram = 27
        }
    }

