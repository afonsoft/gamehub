using Abp.Application.Services.Dto;
using System.Collections.Generic;

namespace GameHub.Administration.OrganizationUnits.Dto
{
    /// <summary>
    /// DTO para representar uma unidade organizacional.
    /// </summary>
    public class OrganizationUnitDto : EntityDto<long>
    {
        /// <summary>
        /// Nome de exibição da unidade organizacional.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Código hierárquico da unidade organizacional.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Identificador da unidade pai.
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// Filhos da unidade organizacional.
        /// </summary>
        public List<OrganizationUnitDto> Children { get; set; }
    }
}
