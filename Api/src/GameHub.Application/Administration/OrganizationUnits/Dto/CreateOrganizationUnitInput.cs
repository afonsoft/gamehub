using System.ComponentModel.DataAnnotations;

namespace GameHub.Administration.OrganizationUnits.Dto
{
    /// <summary>
    /// Entrada para criação de uma unidade organizacional.
    /// </summary>
    public class CreateOrganizationUnitInput
    {
        /// <summary>
        /// Nome de exibição da unidade organizacional.
        /// </summary>
        [Required]
        [StringLength(128)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Identificador da unidade pai.
        /// </summary>
        public long? ParentId { get; set; }
    }
}
