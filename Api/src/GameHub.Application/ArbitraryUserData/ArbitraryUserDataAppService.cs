using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.ArbitraryUserData.Dto;
using GameHub.Monitoring;
using Microsoft.EntityFrameworkCore;

namespace GameHub.ArbitraryUserData
{
    /// <summary>
    /// Application service for arbitrary key/value JSON storage per game/user.
    /// </summary>
    [AbpAllowAnonymous]
    public class ArbitraryUserDataAppService : GameHubAppServiceBase, IArbitraryUserDataAppService
    {
        public const long MaxKeys = 100;
        public const long MaxBytesPerValue = 64 * 1024;

        private readonly IRepository<ArbitraryUserDataRecord, Guid> _repository;

        public ArbitraryUserDataAppService(IRepository<ArbitraryUserDataRecord, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<string> GetAsync(GetArbitraryUserDataInput input)
        {
            var userId = AbpSession.UserId;
            var record = await _repository.GetAll()
                .Where(r => r.GameId == input.GameId
                            && r.Key == input.Key
                            && (userId == null || r.UserId == userId)
                            && (string.IsNullOrEmpty(input.AnonymousIdHash) || r.AnonymousIdHash == input.AnonymousIdHash)
                            && (r.ExpiresAt == null || r.ExpiresAt > Clock.Now))
                .OrderByDescending(r => r.CreationTime)
                .FirstOrDefaultAsync();

            return record?.ValueJson ?? "{}";
        }

        public async Task<ArbitraryUserDataSaveResultDto> SetAsync(SetArbitraryUserDataInput input)
        {
            if (input.Key.StartsWith("gamehub_ignore_", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Keys prefixed with 'gamehub_ignore_' are reserved for local-only storage.");
            }

            ValidateJson(input.ValueJson);

            var bytes = Encoding.UTF8.GetByteCount(input.ValueJson ?? string.Empty);
            if (bytes > MaxBytesPerValue)
            {
                throw new InvalidOperationException($"Value exceeds maximum size of {MaxBytesPerValue} bytes.");
            }

            var userId = AbpSession.UserId;
            var query = _repository.GetAll()
                .Where(r => r.GameId == input.GameId
                            && r.Key == input.Key
                            && (userId == null || r.UserId == userId)
                            && (string.IsNullOrEmpty(input.AnonymousIdHash) || r.AnonymousIdHash == input.AnonymousIdHash));

            var existing = await query.OrderByDescending(r => r.CreationTime).FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.ValueJson = input.ValueJson ?? string.Empty;
                existing.ExpiresAt = input.TtlSeconds.HasValue ? Clock.Now.AddSeconds(input.TtlSeconds.Value) : (DateTime?)null;
                await _repository.UpdateAsync(existing);
                return new ArbitraryUserDataSaveResultDto
                {
                    Saved = true,
                    Quota = await GetQuotaAsync(input.GameId)
                };
            }

            var currentKeyCount = await _repository.GetAll()
                .Where(r => r.GameId == input.GameId
                            && (userId == null || r.UserId == userId)
                            && (string.IsNullOrEmpty(input.AnonymousIdHash) || r.AnonymousIdHash == input.AnonymousIdHash))
                .CountAsync();

            if (currentKeyCount >= MaxKeys)
            {
                throw new InvalidOperationException($"Maximum number of keys ({MaxKeys}) reached for this game/user.");
            }

            await _repository.InsertAsync(new ArbitraryUserDataRecord
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                UserId = userId,
                AnonymousIdHash = input.AnonymousIdHash,
                Key = input.Key,
                ValueJson = input.ValueJson ?? string.Empty,
                ExpiresAt = input.TtlSeconds.HasValue ? Clock.Now.AddSeconds(input.TtlSeconds.Value) : null
            });
            GameHubMetrics.AudsKeysStored.Add(1);
            GameHubMetrics.AudsBytesStored.Add(bytes);

            return new ArbitraryUserDataSaveResultDto
            {
                Saved = true,
                Quota = await GetQuotaAsync(input.GameId)
            };
        }

        public async Task DeleteAsync(DeleteArbitraryUserDataInput input)
        {
            var userId = AbpSession.UserId;
            var record = await _repository.GetAll()
                .Where(r => r.GameId == input.GameId
                            && r.Key == input.Key
                            && (userId == null || r.UserId == userId)
                            && (string.IsNullOrEmpty(input.AnonymousIdHash) || r.AnonymousIdHash == input.AnonymousIdHash))
                .OrderByDescending(r => r.CreationTime)
                .FirstOrDefaultAsync();

            if (record != null)
            {
                await _repository.DeleteAsync(record);
            }
        }

        public async Task<ArbitraryUserDataQuotaDto> GetQuotaAsync(Guid gameId)
        {
            var userId = AbpSession.UserId;
            var records = await _repository.GetAll()
                .Where(r => r.GameId == gameId
                            && (userId == null || r.UserId == userId)
                            && (r.ExpiresAt == null || r.ExpiresAt > Clock.Now))
                .ToListAsync();

            var totalBytes = records.Sum(r => string.IsNullOrEmpty(r.ValueJson) ? 0 : Encoding.UTF8.GetByteCount(r.ValueJson));

            return new ArbitraryUserDataQuotaDto
            {
                GameId = gameId,
                TotalKeys = records.Count,
                TotalBytes = totalBytes,
                MaxKeys = MaxKeys,
                MaxBytesPerValue = MaxBytesPerValue
            };
        }

        private static void ValidateJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            try
            {
                JsonDocument.Parse(value);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Value is not a valid JSON document.", ex);
            }
        }
    }
}
