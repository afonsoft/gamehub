using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using GameHub.Authorization;
using GameHub.Moderation.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Moderation
{
    [AbpAuthorize]
    public class UserContentAppService : GameHubAppServiceBase, IUserContentAppService
    {
        private readonly IRepository<UserContent, Guid> _contentRepository;
        private readonly ProfanityFilter _profanityFilter;

        public UserContentAppService(IRepository<UserContent, Guid> contentRepository)
        {
            _contentRepository = contentRepository;
            _profanityFilter = new ProfanityFilter();
        }

        public async Task<UserContentDto> SubmitAsync(SubmitUserContentInput input)
        {
            var hasProfanity = _profanityFilter.ContainsProfanity(input.Text);

            var content = new UserContent
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                UserId = AbpSession.UserId,
                ContentType = input.ContentType,
                Text = input.Text,
                IsApproved = !hasProfanity,
                RequiresModeration = hasProfanity,
                ModerationReason = hasProfanity ? "Contains profanity" : null
            };

            await _contentRepository.InsertAsync(content);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<UserContentDto>(content);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_View)]
        public async Task<List<UserContentDto>> GetPendingAsync(int maxResultCount = 50)
        {
            var items = await _contentRepository.GetAll()
                .Where(c => !c.IsApproved || c.RequiresModeration)
                .OrderByDescending(c => c.CreationTime)
                .Take(maxResultCount)
                .ToListAsync();

            return ObjectMapper.Map<List<UserContentDto>>(items);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_View)]
        public async Task<UserContentDto> ModerateAsync(ModerateUserContentInput input)
        {
            var content = await _contentRepository.GetAsync(input.ContentId);
            content.IsApproved = input.IsApproved;
            content.RequiresModeration = false;
            content.ModerationReason = input.Reason;

            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<UserContentDto>(content);
        }

        public async Task<List<UserContentDto>> GetByGameAsync(Guid gameId, bool onlyApproved = true)
        {
            var query = _contentRepository.GetAll().Where(c => c.GameId == gameId);
            if (onlyApproved)
            {
                query = query.Where(c => c.IsApproved && !c.RequiresModeration);
            }

            var items = await query.OrderByDescending(c => c.CreationTime).ToListAsync();
            return ObjectMapper.Map<List<UserContentDto>>(items);
        }
    }
}
