using System.ComponentModel.DataAnnotations;

namespace GameHub.Administration.OrganizationUnits.Dto
{
    /// <summary>
    /// Entrada para atualização de uma unidade organizacional.
    /// </summary>
    public class UpdateOrganizationUnitInput
    {
        /// <summary>
        /// Identificador da unidade organizacional.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome de exibição da unidade organizacional.
        /// </summary>
        [Required]
        [StringLength(128)]
        public string DisplayName { get; set; }
    }
}
