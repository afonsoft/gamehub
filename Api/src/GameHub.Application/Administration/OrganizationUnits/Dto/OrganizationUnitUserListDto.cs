namespace GameHub.Administration.OrganizationUnits.Dto
{
    /// <summary>
    /// DTO para representar um usuário em uma unidade organizacional.
    /// </summary>
    public class OrganizationUnitUserListDto
    {
        /// <summary>
        /// Identificador do usuário.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Nome de usuário.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Nome.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sobrenome.
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Endereço de e-mail.
        /// </summary>
        public string EmailAddress { get; set; }
    }
}
