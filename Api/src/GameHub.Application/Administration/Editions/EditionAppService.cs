using Abp.Application.Editions;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using GameHub.Administration.Editions.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace GameHub.Administration.Editions
{
    /// <summary>
    /// Serviço de aplicação para gerenciamento de Editions.
    /// </summary>
    [AbpAuthorize("Pages.Administration.Editions")]
    public class EditionAppService : GameHubAppServiceBase, IEditionAppService
    {
        private readonly IRepository<Edition> _editionRepository;

        /// <summary>
        /// EditionAppService.
        /// </summary>
        /// <param name="editionRepository">Repositório de edições.</param>
        public EditionAppService(IRepository<Edition> editionRepository)
        {
            _editionRepository = editionRepository;
        }

        /// <summary>
        /// Obtém as edições paginadas.
        /// </summary>
        /// <param name="input">Filtros e paginação.</param>
        /// <returns>Lista paginada de edições.</returns>
        public async Task<PagedResultDto<EditionDto>> GetEditions(GetEditionsInput input)
        {
            var query = (await _editionRepository.GetAllAsync())
                .WhereIf(!input.Filter.IsNullOrWhiteSpace(), e => e.DisplayName.Contains(input.Filter));

            var total = await query.CountAsync();
            var ordered = DynamicQueryableExtensions.OrderBy(query, input.Sorting ?? "DisplayName");
            var editions = await ordered.PageBy(input).ToListAsync();

            return new PagedResultDto<EditionDto>(total, ObjectMapper.Map<List<EditionDto>>(editions));
        }

        /// <summary>
        /// Obtém uma edição para edição.
        /// </summary>
        /// <param name="input">Identificador da edição.</param>
        /// <returns>Edição encontrada.</returns>
        public async Task<EditionDto> GetEditionForEdit(EntityDto input)
        {
            var edition = await _editionRepository.GetAsync(input.Id);
            return ObjectMapper.Map<EditionDto>(edition);
        }

        /// <summary>
        /// Cria uma nova edição.
        /// </summary>
        /// <param name="input">Dados da edição.</param>
        /// <returns>Task.</returns>
        [AbpAuthorize("Pages.Administration.Editions.Create")]
        public async Task CreateEdition(CreateEditionInput input)
        {
            var edition = ObjectMapper.Map<Edition>(input);
            await _editionRepository.InsertAsync(edition);
        }

        /// <summary>
        /// Atualiza uma edição existente.
        /// </summary>
        /// <param name="input">Dados da edição.</param>
        /// <returns>Task.</returns>
        [AbpAuthorize("Pages.Administration.Editions.Edit")]
        public async Task UpdateEdition(UpdateEditionInput input)
        {
            var edition = await _editionRepository.GetAsync(input.Id);
            ObjectMapper.Map(input, edition);
            await _editionRepository.UpdateAsync(edition);
        }

        /// <summary>
        /// Remove uma edição.
        /// </summary>
        /// <param name="input">Identificador da edição.</param>
        /// <returns>Task.</returns>
        [AbpAuthorize("Pages.Administration.Editions.Delete")]
        public async Task DeleteEdition(EntityDto input)
        {
            await _editionRepository.DeleteAsync(input.Id);
        }
    }
}
