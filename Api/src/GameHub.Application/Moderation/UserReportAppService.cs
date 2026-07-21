using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using GameHub.Admin.Dto;
using GameHub.Catalog;
using GameHub.Moderation.Dto;

namespace GameHub.Moderation
{
    /// <summary>
    /// Permite que jogadores reportem jogos ou conteúdo inadequado.
    /// </summary>
    public class UserReportAppService : ApplicationService, IUserReportAppService
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
    }
}
