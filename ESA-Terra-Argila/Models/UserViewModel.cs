namespace ESA_Terra_Argila.ViewModels
{
    /// <summary>
    /// Representa os dados do utilizador para exibição na view de aprovação.
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// Identificador do utilizador.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nome completo do utilizador.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Email do utilizador.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Role do utilizador.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Indica se o utilizador está bloqueado atualmente.
        /// </summary>
        public bool IsLocked { get; set; }
    }
}

