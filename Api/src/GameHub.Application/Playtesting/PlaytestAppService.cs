using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using Abp.UI;
using GameHub.Playtesting.Dto;
using GameHub.Authorization;
using GameHub.Catalog;
using GameHub.Developers;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Playtesting
{
    public class PlaytestAppService : GameHubAppServiceBase, IPlaytestAppService
    {
        private readonly IRepository<PlaytestSession, Guid> _playtestRepository;
        private readonly IRepository<PlaytestRecording, Guid> _recordingRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<DeveloperTeamMember, Guid> _teamMemberRepository;

        public PlaytestAppService(
            IRepository<PlaytestSession, Guid> playtestRepository,
            IRepository<PlaytestRecording, Guid> recordingRepository,
            IRepository<Game, Guid> gameRepository,
            IRepository<DeveloperTeamMember, Guid> teamMemberRepository)
        {
            _playtestRepository = playtestRepository;
            _recordingRepository = recordingRepository;
            _gameRepository = gameRepository;
            _teamMemberRepository = teamMemberRepository;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Developer_Games)]
        public async Task<PlaytestSessionDto> RequestPlaytestAsync(RequestPlaytestInput input)
        {
            await EnsureCurrentUserHasAccessToGameAsync(input.GameId);

            var playtest = new PlaytestSession
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                RequestedByUserId = AbpSession.UserId.Value,
                Status = PlaytestSessionStatus.Requested,
                Notes = input.Notes,
                CreatedAt = Clock.Now
            };

            await _playtestRepository.InsertAsync(playtest);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<PlaytestSessionDto>(playtest);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Developer_Games)]
        public async Task<ListResultDto<PlaytestSessionDto>> GetPlaytestsByGameAsync(Guid gameId)
        {
            await EnsureCurrentUserHasAccessToGameAsync(gameId);

            var sessions = await _playtestRepository.GetAll()
                .Where(p => p.GameId == gameId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return new ListResultDto<PlaytestSessionDto>(ObjectMapper.Map<List<PlaytestSessionDto>>(sessions));
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_Review)]
        public async Task<PlaytestSessionDto> UploadRecordingAsync(UploadPlaytestRecordingInput input)
        {
            var playtest = await _playtestRepository.GetAsync(input.PlaytestId);
            playtest.RecordingUrl = input.RecordingUrl;
            playtest.Status = PlaytestSessionStatus.Completed;
            playtest.CompletedAt = Clock.Now;

            var recording = new PlaytestRecording
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                PlaytestSessionId = playtest.Id,
                Url = input.RecordingUrl,
                DurationSeconds = input.DurationSeconds,
                DeviceType = input.DeviceType,
                CountryCode = input.CountryCode,
                ConsoleOutput = input.ConsoleOutput,
                LevelEvents = input.LevelEvents
            };

            await _recordingRepository.InsertAsync(recording);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<PlaytestSessionDto>(playtest);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_Review)]
        public async Task<PlaytestRecordingDto> GetRecordingAsync(Guid recordingId)
        {
            var recording = await _recordingRepository.GetAsync(recordingId);
            return ObjectMapper.Map<PlaytestRecordingDto>(recording);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_Review)]
        public async Task<ListResultDto<PlaytestRecordingDto>> ListRecordingsAsync(Guid playtestId)
        {
            var recordings = await _recordingRepository.GetAll()
                .Where(r => r.PlaytestSessionId == playtestId)
                .OrderByDescending(r => r.CreationTime)
                .ToListAsync();

            return new ListResultDto<PlaytestRecordingDto>(ObjectMapper.Map<List<PlaytestRecordingDto>>(recordings));
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_Review)]
        public async Task<PagedResultDto<PlaytestRecordingDto>> GetAllRecordingsAsync(GetAllPlaytestRecordingsInput input)
        {
            var query = _recordingRepository.GetAll()
                .Include(r => r.PlaytestSession)
                .AsQueryable();

            if (input.GameId.HasValue)
            {
                query = query.Where(r => r.PlaytestSession.GameId == input.GameId.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.DeviceType))
            {
                query = query.Where(r => r.DeviceType == input.DeviceType);
            }

            var totalCount = await query.CountAsync();
            var recordings = await query
                .OrderByDescending(r => r.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<PlaytestRecordingDto>(totalCount, ObjectMapper.Map<List<PlaytestRecordingDto>>(recordings));
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_Review)]
        public async Task<PlaytestRecordingDto> AddNotesAsync(AddPlaytestRecordingNotesInput input)
        {
            var recording = await _recordingRepository.GetAsync(input.RecordingId);
            recording.Notes = input.Notes ?? string.Empty;

            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<PlaytestRecordingDto>(recording);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_Review)]
        public async Task<PlaytestDifficultyInsightDto> GetDifficultyInsightsAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAsync(gameId);
            var recordings = await _recordingRepository.GetAll()
                .Include(r => r.PlaytestSession)
                .Where(r => r.PlaytestSession.GameId == gameId && !string.IsNullOrEmpty(r.LevelEvents))
                .Select(r => r.LevelEvents)
                .ToListAsync();

            var levelGroups = new Dictionary<string, (long Starts, long Deaths, long Restarts, long Completions)>(StringComparer.OrdinalIgnoreCase);

            foreach (var levelEventsJson in recordings)
            {
                if (string.IsNullOrWhiteSpace(levelEventsJson))
                {
                    continue;
                }

                try
                {
                    var events = JsonSerializer.Deserialize<List<PlaytestLevelEvent>>(levelEventsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PlaytestLevelEvent>();
                    foreach (var levelEvent in events)
                    {
                        if (string.IsNullOrWhiteSpace(levelEvent.Level) || string.IsNullOrWhiteSpace(levelEvent.Event))
                        {
                            continue;
                        }

                        var current = levelGroups.GetValueOrDefault(levelEvent.Level);
                        current.Starts += levelEvent.Event.Equals("start", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                        current.Deaths += levelEvent.Event.Equals("death", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                        current.Restarts += levelEvent.Event.Equals("restart", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                        current.Completions += levelEvent.Event.Equals("complete", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                        levelGroups[levelEvent.Level] = current;
                    }
                }
                catch (JsonException)
                {
                    // Ignore malformed JSON.
                }
            }

            var levels = levelGroups
                .Select(g =>
                {
                    var (starts, deaths, restarts, completions) = g.Value;
                    return new LevelDifficultyDto
                    {
                        Level = g.Key,
                        Starts = starts,
                        Deaths = deaths,
                        Restarts = restarts,
                        Completions = completions,
                        CompletionRate = starts > 0 ? (double)completions / starts : 0.0,
                        AvgDeathsPerStart = starts > 0 ? (double)deaths / starts : 0.0
                    };
                })
                .OrderBy(l => l.Level)
                .ToList();

            return new PlaytestDifficultyInsightDto
            {
                GameId = gameId,
                GameTitle = game.Title,
                Levels = levels
            };
        }

        private class PlaytestLevelEvent
        {
            public string Level { get; set; }
            public string Event { get; set; }
        }

        private async Task EnsureCurrentUserHasAccessToGameAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAsync(gameId);

            var teamMember = await _teamMemberRepository.FirstOrDefaultAsync(m => m.UserId == AbpSession.UserId.Value);
            if (teamMember == null)
            {
                throw new AbpAuthorizationException("You must belong to a developer team to manage playtests.");
            }

            // Future: authorize by team ownership. For now, any team member can request a playtest for any game.
            // This keeps 24.2 decoupled from the game-team relationship until 24.1 is fully wired.
        }
    }
}
