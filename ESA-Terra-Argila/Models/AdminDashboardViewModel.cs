namespace ESA_Terra_Argila.Models
{
    public class AdminDashboardViewModel
    {
        //Contagens gerais
        public int TotalUsers { get; set; }
        public int ApprovedUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalMaterials { get; set; }
        public int TotalOrders { get; set; }

        //Utilizadores únicos 
        public int UniqueUsers24h { get; set; }
        public int UniqueUsers7d { get; set; }
        public int UniqueUsersMonth { get; set; }
        public int UniqueUsersYear { get; set; }
        public int UniqueUsersTotal { get; set; }

        //Utilizadores ativos 
        public int ActiveUsers24h { get; set; }
        public int ActiveUsers7d { get; set; }
        public int ActiveUsersMonth { get; set; }
        public int ActiveUsersYear { get; set; }
        public int ActiveUsersTotal { get; set; }
    }
}
