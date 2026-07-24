using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Timing;
using Abp.UI;
using GameHub.Gameplay.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Gameplay
{
    /// <summary>
    /// Persists and retrieves cloud saves for logged-in users. Anonymous players keep saves locally.
    /// </summary>
    public class CloudSaveAppService : GameHubAppServiceBase, ICloudSaveAppService
    {
        private const long MaxSaveBytes = 1_048_576; // 1 MB
        private readonly IRepository<CloudSave, Guid> _cloudSaveRepository;

        public CloudSaveAppService(IRepository<CloudSave, Guid> cloudSaveRepository)
        {
            _cloudSaveRepository = cloudSaveRepository;
        }

        public async Task<CloudSaveDto> GetAsync(GetCloudSaveInput input)
        {
            var deviceHash = HashDeviceId(input.DeviceId);
            var query = await _cloudSaveRepository.GetAll()
                .Where(s => s.GameId == input.GameId)
                .Where(s => AbpSession.UserId.HasValue ? s.UserId == AbpSession.UserId.Value : s.DeviceIdHash == deviceHash)
                .FirstOrDefaultAsync();

            if (query == null)
            {
                return new CloudSaveDto { GameId = input.GameId };
            }

            return new CloudSaveDto
            {
                GameId = query.GameId,
                Data = query.Data,
                LastModificationTime = query.LastModificationTime,
            };
        }

        public async Task<CloudSaveDto> SaveAsync(SaveCloudSaveInput input)
        {
            var size = Encoding.UTF8.GetByteCount(input.Data);
            if (size > MaxSaveBytes)
            {
                throw new UserFriendlyException("Save data exceeds the 1 MB limit.");
            }

            var deviceHash = HashDeviceId(input.DeviceId);
            var existing = await _cloudSaveRepository.GetAll()
                .Where(s => s.GameId == input.GameId)
                .Where(s => AbpSession.UserId.HasValue ? s.UserId == AbpSession.UserId.Value : s.DeviceIdHash == deviceHash)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                existing = new CloudSave
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    GameId = input.GameId,
                    UserId = AbpSession.UserId,
                    DeviceIdHash = deviceHash,
                    CreationTime = Clock.Now,
                };
                await _cloudSaveRepository.InsertAsync(existing);
            }

            existing.Data = input.Data;
            existing.UncompressedSize = size;
            existing.LastModificationTime = Clock.Now;

            await CurrentUnitOfWork.SaveChangesAsync();

            return new CloudSaveDto
            {
                GameId = existing.GameId,
                Data = existing.Data,
                LastModificationTime = existing.LastModificationTime,
            };
        }

        public async Task DeleteAsync(GetCloudSaveInput input)
        {
            var deviceHash = HashDeviceId(input.DeviceId);
            var existing = await _cloudSaveRepository.GetAll()
                .Where(s => s.GameId == input.GameId)
                .Where(s => AbpSession.UserId.HasValue ? s.UserId == AbpSession.UserId.Value : s.DeviceIdHash == deviceHash)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                await _cloudSaveRepository.DeleteAsync(existing);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }

        private static string HashDeviceId(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(deviceId);
            var hash = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
