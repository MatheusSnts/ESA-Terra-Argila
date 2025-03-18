namespace ESA_Terra_Argila.Models
{
    public class Invitation
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool Used { get; set; }
    }
}