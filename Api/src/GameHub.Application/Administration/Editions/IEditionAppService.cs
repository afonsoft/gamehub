using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Administration.Editions.Dto;
using System.Threading.Tasks;

namespace GameHub.Administration.Editions
{
    /// <summary>
    /// Contrato do serviço de aplicação para gerenciamento de Editions.
    /// </summary>
    public interface IEditionAppService : IApplicationService
    {
        /// <summary>
        /// Obtém as edições paginadas.
        /// </summary>
        /// <param name="input">Filtros e paginação.</param>
        /// <returns>Lista paginada de edições.</returns>
        Task<PagedResultDto<EditionDto>> GetEditions(GetEditionsInput input);

        /// <summary>
        /// Obtém uma edição para edição.
        /// </summary>
        /// <param name="input">Identificador da edição.</param>
        /// <returns>Edição encontrada.</returns>
        Task<EditionDto> GetEditionForEdit(EntityDto input);

        /// <summary>
        /// Cria uma nova edição.
        /// </summary>
        /// <param name="input">Dados da edição.</param>
        /// <returns>Task.</returns>
        Task CreateEdition(CreateEditionInput input);

        /// <summary>
        /// Atualiza uma edição existente.
        /// </summary>
        /// <param name="input">Dados da edição.</param>
        /// <returns>Task.</returns>
        Task UpdateEdition(UpdateEditionInput input);

        /// <summary>
        /// Remove uma edição.
        /// </summary>
        /// <param name="input">Identificador da edição.</param>
        /// <returns>Task.</returns>
        Task DeleteEdition(EntityDto input);
    }
}
