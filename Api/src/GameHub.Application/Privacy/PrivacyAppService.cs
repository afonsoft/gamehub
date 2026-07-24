using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using Eaf.Middleware.Authorization.Users;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Moderation;
using GameHub.Privacy.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameHub.Privacy
{
    /// <summary>
    /// Implements LGPD operations for data portability and deletion/anonymization.
    /// </summary>
    [AbpAuthorize]
    public class PrivacyAppService : GameHubAppServiceBase, IPrivacyAppService
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<PlayerPrivacyConsent, Guid> _privacyConsentRepository;
        private readonly IRepository<PlaySession, Guid> _playSessionRepository;
        private readonly IRepository<LeaderboardEntry, Guid> _leaderboardEntryRepository;
        private readonly IRepository<UserReport, Guid> _userReportRepository;

        public PrivacyAppService(
            IRepository<User, long> userRepository,
            IRepository<DeveloperProfile, Guid> developerProfileRepository,
            IRepository<Game, Guid> gameRepository,
            IRepository<PlayerPrivacyConsent, Guid> privacyConsentRepository,
            IRepository<PlaySession, Guid> playSessionRepository,
            IRepository<LeaderboardEntry, Guid> leaderboardEntryRepository,
            IRepository<UserReport, Guid> userReportRepository)
        {
            _userRepository = userRepository;
            _developerProfileRepository = developerProfileRepository;
            _gameRepository = gameRepository;
            _privacyConsentRepository = privacyConsentRepository;
            _playSessionRepository = playSessionRepository;
            _leaderboardEntryRepository = leaderboardEntryRepository;
            _userReportRepository = userReportRepository;
        }

        public async Task<UserDataExportDto> ExportUserDataAsync(long userId)
        {
            EnsureCurrentUserOrFail(userId);

            var user = await _userRepository.GetAsync(userId);

            var sessions = await _playSessionRepository.GetAll()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartedAt)
                .Take(1000)
                .ToListAsync();

            var leaderboardEntries = await _leaderboardEntryRepository.GetAll()
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.UpdatedAt)
                .Take(1000)
                .ToListAsync();

            var reports = await _userReportRepository.GetAll()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreationTime)
                .Take(1000)
                .ToListAsync();

            var developerProfile = await _developerProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId);

            return new UserDataExportDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                EmailAddress = user.EmailAddress,
                Name = user.Name,
                CreationTime = user.CreationTime,
                PlaySessions = sessions.Select(s => new PlaySessionExportDto
                {
                    SessionId = s.Id,
                    GameId = s.GameId,
                    StartedAt = s.StartedAt,
                    EndedAt = s.EndedAt,
                    DeviceType = s.DeviceType,
                    Browser = s.Browser,
                    CountryCode = s.CountryCode,
                }).ToList(),
                LeaderboardEntries = leaderboardEntries.Select(e => new LeaderboardEntryExportDto
                {
                    GameId = e.GameId,
                    Score = e.Score,
                    CreatedAt = e.CreatedAt,
                }).ToList(),
                UserReports = reports.Select(r => new UserReportExportDto
                {
                    ReportId = r.Id,
                    GameId = r.GameId,
                    Reason = r.Reason,
                    Description = r.Description,
                    CreationTime = r.CreationTime,
                }).ToList(),
                DeveloperProfile = developerProfile == null ? null : new DeveloperProfileExportDto
                {
                    DisplayName = developerProfile.DisplayName,
                    LegalName = developerProfile.LegalName,
                    WebsiteUrl = developerProfile.WebsiteUrl,
                    SupportEmail = developerProfile.SupportEmail,
                },
            };
        }

        public async Task DeleteUserDataAsync(long userId)
        {
            EnsureCurrentUserOrFail(userId);

            var user = await _userRepository.GetAsync(userId);

            await AnonymizePlaySessionsAsync(userId);
            await AnonymizeUserReportsAsync(userId);
            await AnonymizeDeveloperProfileAsync(userId);
            await AnonymizeUserAsync(user);
        }

        private void EnsureCurrentUserOrFail(long userId)
        {
            if (AbpSession.UserId != userId)
            {
                throw new AbpAuthorizationException("You are not authorized to access this user's data.");
            }
        }

        private async Task AnonymizeUserAsync(User user)
        {
            var deletedSuffix = $"deleted-{user.Id}";

            user.Name = "Deleted User";
            user.UserName = deletedSuffix;
            user.EmailAddress = $"{deletedSuffix}@gamehub.local";
            user.PhoneNumber = string.Empty;
            user.IsActive = false;
            user.IsDeleted = true;

            if (!string.IsNullOrEmpty(user.NormalizedUserName))
                user.NormalizedUserName = deletedSuffix.ToUpperInvariant();

            if (!string.IsNullOrEmpty(user.NormalizedEmailAddress))
                user.NormalizedEmailAddress = $"{deletedSuffix}@gamehub.local".ToUpperInvariant();

            await _userRepository.UpdateAsync(user);
        }

        private async Task AnonymizePlaySessionsAsync(long userId)
        {
            var sessions = await _playSessionRepository.GetAll()
                .Where(s => s.UserId == userId)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.UserId = null;
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task AnonymizeUserReportsAsync(long userId)
        {
            var reports = await _userReportRepository.GetAll()
                .Where(r => r.UserId == userId)
                .ToListAsync();

            foreach (var report in reports)
            {
                report.UserId = null;
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task AnonymizeDeveloperProfileAsync(long userId)
        {
            var profile = await _developerProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                return;
            }

            profile.DisplayName = "Deleted User";
            profile.LegalName = string.Empty;
            profile.WebsiteUrl = string.Empty;
            profile.SupportEmail = string.Empty;
            profile.Status = DeveloperProfileStatus.Suspended;

            await _developerProfileRepository.UpdateAsync(profile);
        }

        [AbpAllowAnonymous]
        public virtual async Task<PrivacyPolicyDto> GetForGameAsync(string gameSlug)
        {
            var game = await _gameRepository.GetAll()
                .Select(g => new { g.Id, g.Slug, g.PrivacyPolicyUrl })
                .FirstOrDefaultAsync(g => g.Slug == gameSlug);

            if (game == null)
            {
                throw new AbpAuthorizationException("Game not found");
            }

            return new PrivacyPolicyDto
            {
                GameSlug = game.Slug,
                Url = game.PrivacyPolicyUrl ?? string.Empty,
                Text = string.Empty,
                RequiresConsent = !string.IsNullOrWhiteSpace(game.PrivacyPolicyUrl)
            };
        }

        public virtual async Task SaveConsentAsync(SavePrivacyConsentInput input)
        {
            if (!AbpSession.UserId.HasValue)
            {
                throw new AbpAuthorizationException("User must be logged in to consent.");
            }

            var existing = await _privacyConsentRepository.FirstOrDefaultAsync(c =>
                c.GameId == input.GameId && c.UserId == AbpSession.UserId.Value);

            if (existing != null)
            {
                existing.ConsentedAt = Clock.Now;
                existing.PolicyVersion = input.PolicyVersion;
                await _privacyConsentRepository.UpdateAsync(existing);
            }
            else
            {
                await _privacyConsentRepository.InsertAsync(new PlayerPrivacyConsent
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId.Value,
                    GameId = input.GameId,
                    ConsentedAt = Clock.Now,
                    CreationTime = Clock.Now,
                    PolicyVersion = input.PolicyVersion
                });
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
