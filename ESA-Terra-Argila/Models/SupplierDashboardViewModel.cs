namespace ESA_Terra_Argila.Models
{
    public class SupplierDashboardViewModel
    {
        public string NomeFornecedor { get; set; }
        public int TotalMateriais { get; set; }
        public float StockTotal { get; set; }

        public string MaterialMaisFavorito { get; set; }
        public int FavoritosDoMaisPopular { get; set; }

        public string MaterialMaisVendido { get; set; }
        public float QuantidadeVendida { get; set; }

    }
}
