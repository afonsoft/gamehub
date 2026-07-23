using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using GameHub.Admin.Dto;
using GameHub.Authorization;
using GameHub.Catalog;
using GameHub.Moderation.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Moderation
{
    /// <summary>
    /// Permite que jogadores reportem jogos ou conteúdo inadequado.
    /// </summary>
    public class UserReportAppService : GameHubAppServiceBase, IUserReportAppService
    {
        private readonly IRepository<UserReport, Guid> _userReportRepository;
        private readonly IRepository<Game, Guid> _gameRepository;

        public UserReportAppService(
            IRepository<UserReport, Guid> userReportRepository,
            IRepository<Game, Guid> gameRepository)
        {
            _userReportRepository = userReportRepository;
            _gameRepository = gameRepository;
        }

        public async Task<UserReportDto> SubmitAsync(UserReportInput input)
        {
            await _gameRepository.GetAsync(input.GameId);

            var report = new UserReport
            {
                Id = Guid.NewGuid(),
                GameId = input.GameId,
                UserId = AbpSession.UserId,
                Reason = input.Reason,
                Description = input.Description,
                Status = UserReportStatus.Open
            };

            await _userReportRepository.InsertAsync(report);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<UserReportDto>(report);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Reports_Manage)]
        public async Task<PagedResultDto<UserReportDto>> GetAllAsync(GetReportsInput input)
        {
            var query = _userReportRepository.GetAll().Where(r => !r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(input.Status) && Enum.TryParse<UserReportStatus>(input.Status, true, out var status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (input.GameId.HasValue)
            {
                query = query.Where(r => r.GameId == input.GameId.Value);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .Include(r => r.Game)
                .ToListAsync();

            return new PagedResultDto<UserReportDto>(total, ObjectMapper.Map<List<UserReportDto>>(items));
        }
    }
}
