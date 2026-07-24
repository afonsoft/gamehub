using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using GameHub.Authorization;
using GameHub.Inspector.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Inspector
{
    [AbpAuthorize(GameHubPermissions.Pages_Builds_View)]
    public class InspectorAppService : GameHubAppServiceBase, IInspectorAppService
    {
        private readonly IRepository<InspectorSession, Guid> _sessionRepository;
        private readonly IRepository<InspectorSdkEvent, Guid> _eventRepository;
        private readonly IRepository<InspectorWarning, Guid> _warningRepository;

        private static readonly string[] ExpectedSdkSequence = new[]
        {
            "gameLoadingStarted",
            "gameLoadingFinished",
            "gameplayStart"
        };

        public InspectorAppService(
            IRepository<InspectorSession, Guid> sessionRepository,
            IRepository<InspectorSdkEvent, Guid> eventRepository,
            IRepository<InspectorWarning, Guid> warningRepository)
        {
            _sessionRepository = sessionRepository;
            _eventRepository = eventRepository;
            _warningRepository = warningRepository;
        }

        public async Task<InspectorSessionDto> StartSessionAsync(StartInspectorSessionInput input)
        {
            var session = new InspectorSession
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                GameBuildId = input.GameBuildId,
                StartedAt = DateTime.UtcNow,
                DevicePreset = input.DevicePreset,
                Resolution = input.Resolution
            };

            await _sessionRepository.InsertAsync(session);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<InspectorSessionDto>(session);
        }

        public async Task LogSdkEventAsync(LogSdkEventInput input)
        {
            var ev = new InspectorSdkEvent
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                SessionId = input.SessionId,
                EventType = input.EventType,
                Payload = input.Payload,
                SequenceNumber = input.SequenceNumber,
                Timestamp = DateTime.UtcNow
            };

            await _eventRepository.InsertAsync(ev);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task AddWarningAsync(AddInspectorWarningInput input)
        {
            var warning = new InspectorWarning
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                SessionId = input.SessionId,
                Category = input.Category,
                Message = input.Message,
                Severity = input.Severity
            };

            await _warningRepository.InsertAsync(warning);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task<InspectorSessionDetailDto> GetSessionAsync(Guid sessionId)
        {
            var session = await _sessionRepository.GetAll()
                .Where(s => s.Id == sessionId)
                .FirstOrDefaultAsync();

            if (session == null)
            {
                return null;
            }

            var events = await _eventRepository.GetAll()
                .Where(e => e.SessionId == sessionId)
                .OrderBy(e => e.SequenceNumber)
                .ToListAsync();

            var warnings = await _warningRepository.GetAll()
                .Where(w => w.SessionId == sessionId)
                .ToListAsync();

            return new InspectorSessionDetailDto
            {
                Id = session.Id,
                GameId = session.GameId,
                GameBuildId = session.GameBuildId,
                StartedAt = session.StartedAt,
                DevicePreset = session.DevicePreset,
                Resolution = session.Resolution,
                Status = session.Status,
                Events = ObjectMapper.Map<List<InspectorSdkEventDto>>(events),
                Warnings = ObjectMapper.Map<List<InspectorWarningDto>>(warnings)
            };
        }

        public async Task<List<InspectorSessionDto>> GetSessionsAsync(Guid gameId, int maxResultCount = 20)
        {
            var sessions = await _sessionRepository.GetAll()
                .Where(s => s.GameId == gameId)
                .OrderByDescending(s => s.StartedAt)
                .Take(maxResultCount)
                .ToListAsync();

            return ObjectMapper.Map<List<InspectorSessionDto>>(sessions);
        }

        public async Task<List<InspectorWarningDto>> ValidateSessionAsync(Guid sessionId)
        {
            var events = await _eventRepository.GetAll()
                .Where(e => e.SessionId == sessionId)
                .OrderBy(e => e.SequenceNumber)
                .ToListAsync();

            var warnings = new List<InspectorWarningDto>();

            ValidateEventSequence(events, warnings);
            ValidateAdBreakEvents(events, warnings);
            ValidateDuplicateEvents(events, warnings);

            foreach (var w in warnings)
            {
                await AddWarningAsync(new AddInspectorWarningInput
                {
                    SessionId = sessionId,
                    Category = "SDK",
                    Message = w.Message,
                    Severity = w.Severity
                });
            }

            return warnings;
        }

        private static void ValidateEventSequence(List<InspectorSdkEvent> events, List<InspectorWarningDto> warnings)
        {
            var orderedEvents = events.OrderBy(e => e.SequenceNumber).Select(e => e.EventType).ToList();

            var loadingStartedIndex = orderedEvents.IndexOf("gameLoadingStarted");
            var loadingFinishedIndex = orderedEvents.IndexOf("gameLoadingFinished");
            var gameplayStartIndex = orderedEvents.IndexOf("gameplayStart");

            if (gameplayStartIndex >= 0 && (loadingFinishedIndex < 0 || gameplayStartIndex < loadingFinishedIndex))
            {
                warnings.Add(new InspectorWarningDto
                {
                    Category = "Order",
                    Message = "gameplayStart was called before gameLoadingFinished.",
                    Severity = "Critical"
                });
            }

            if (loadingFinishedIndex >= 0 && loadingStartedIndex >= 0 && loadingFinishedIndex < loadingStartedIndex)
            {
                warnings.Add(new InspectorWarningDto
                {
                    Category = "Order",
                    Message = "gameLoadingFinished was called before gameLoadingStarted.",
                    Severity = "Critical"
                });
            }

            if (loadingStartedIndex < 0 && loadingFinishedIndex >= 0)
            {
                warnings.Add(new InspectorWarningDto
                {
                    Category = "Order",
                    Message = "gameLoadingStarted was not received before gameLoadingFinished.",
                    Severity = "Warning"
                });
            }
        }

        private static void ValidateAdBreakEvents(List<InspectorSdkEvent> events, List<InspectorWarningDto> warnings)
        {
            var ordered = events.OrderBy(e => e.SequenceNumber).ToList();
            for (var i = 0; i < ordered.Count; i++)
            {
                if (ordered[i].EventType.Contains("commercial", StringComparison.OrdinalIgnoreCase) ||
                    ordered[i].EventType.Contains("rewarded", StringComparison.OrdinalIgnoreCase))
                {
                    if (i > 1 && ordered[i - 1].EventType != "adBreakMute" && ordered[i - 1].EventType != "adBreakUnmute")
                    {
                        warnings.Add(new InspectorWarningDto
                        {
                            Category = "AdBreak",
                            Message = $"{ordered[i].EventType} was not surrounded by adBreakMute/adBreakUnmute events.",
                            Severity = "Warning"
                        });
                    }
                }
            }
        }

        private static void ValidateDuplicateEvents(List<InspectorSdkEvent> events, List<InspectorWarningDto> warnings)
        {
            var duplicates = events
                .GroupBy(e => e.EventType)
                .Where(g => g.Count() > 1 && g.Key != "gameMeasuredEvent")
                .Select(g => g.Key)
                .ToList();

            foreach (var dup in duplicates)
            {
                warnings.Add(new InspectorWarningDto
                {
                    Category = "Duplicate",
                    Message = $"{dup} was emitted more than once.",
                    Severity = "Warning"
                });
            }
        }
    }
}
