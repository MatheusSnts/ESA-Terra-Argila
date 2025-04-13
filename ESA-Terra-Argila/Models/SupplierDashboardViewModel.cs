namespace ESA_Terra_Argila.Models
{
    public class SupplierDashboardViewModel
    {
        public string SupplierName { get; set; }

        public int TotalMaterials { get; set; }

        public float TotalStock { get; set; }

        public string MostFavoritedMaterial { get; set; }

        public int MostFavoritedCount { get; set; }

        public string BestSellingMaterial { get; set; }

        public int BestSellingQuantity { get; set; }

        public decimal TotalRevenue { get; set; }
    }
}
