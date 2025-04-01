namespace ESA_Terra_Argila.Models
{
    /// <summary>
    /// Classe de configuração para o serviço de email
    /// </summary>
    public class EmailConfiguration
    {
        /// <summary>
        /// Host do servidor de email
        /// </summary>
        public string Host { get; set; } = default!;
        
        /// <summary>
        /// Porta do servidor de email
        /// </summary>
        public int Port { get; set; }
        
        /// <summary>
        /// Nome de usuário para autenticação no servidor de email
        /// </summary>
        public string UserName { get; set; } = default!;
        
        /// <summary>
        /// Senha para autenticação no servidor de email
        /// </summary>
        public string Password { get; set; } = default!;
        
        /// <summary>
        /// Indica se a conexão SSL deve ser habilitada
        /// </summary>
        public bool EnableSsl { get; set; }
        
        /// <summary>
        /// Email do remetente
        /// </summary>
        public string SenderEmail { get; set; } = default!;
        
        /// <summary>
        /// Nome do remetente
        /// </summary>
        public string SenderName { get; set; } = default!;
    }
} 