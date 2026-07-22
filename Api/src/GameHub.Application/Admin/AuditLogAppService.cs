using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Domain.Repositories;
using Eaf.Middleware.Authorization.Users;
using GameHub.Admin.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Admin
{
    /// <summary>
    /// Consulta logs de auditoria do ABP para o painel administrativo.
    /// </summary>
    public class AuditLogAppService : GameHubAppServiceBase, IAuditLogAppService
    {
        private readonly IRepository<AuditLog, long> _auditLogRepository;
        private readonly IRepository<User, long> _userRepository;

        public AuditLogAppService(
            IRepository<AuditLog, long> auditLogRepository,
            IRepository<User, long> userRepository)
        {
            _auditLogRepository = auditLogRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResultDto<AuditLogDto>> GetAllAsync(GetAuditLogsInput input)
        {
            IQueryable<AuditLog> query = _auditLogRepository.GetAll();

            if (input.StartTime.HasValue)
            {
                query = query.Where(a => a.ExecutionTime >= input.StartTime.Value);
            }

            if (input.EndTime.HasValue)
            {
                query = query.Where(a => a.ExecutionTime <= input.EndTime.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.Action))
            {
                var action = input.Action.ToLowerInvariant();
                query = query.Where(a => (a.ServiceName + "." + a.MethodName).ToLower().Contains(action)
                                         || a.ServiceName.ToLower().Contains(action)
                                         || a.MethodName.ToLower().Contains(action));
            }

            List<long> userIds = null;
            if (!string.IsNullOrWhiteSpace(input.UserName))
            {
                var userName = input.UserName.ToLowerInvariant();
                userIds = await _userRepository.GetAll()
                    .Where(u => u.UserName.ToLower().Contains(userName))
                    .Select(u => u.Id)
                    .ToListAsync();

                if (!userIds.Any())
                {
                    return new PagedResultDto<AuditLogDto>(0, new List<AuditLogDto>());
                }

                query = query.Where(a => a.UserId.HasValue && userIds.Contains(a.UserId.Value));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.ExecutionTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var result = ObjectMapper.Map<List<AuditLogDto>>(items);
            await EnrichUserNamesAsync(result);

            return new PagedResultDto<AuditLogDto>(total, result);
        }

        private async Task EnrichUserNamesAsync(List<AuditLogDto> items)
        {
            var userIdSet = items.Where(i => i.UserId.HasValue).Select(i => i.UserId.Value).Distinct().ToList();
            if (!userIdSet.Any())
            {
                return;
            }

            var users = await _userRepository.GetAll()
                .Where(u => userIdSet.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            var lookup = users.ToDictionary(u => u.Id, u => u.UserName);
            foreach (var item in items.Where(i => i.UserId.HasValue))
            {
                if (lookup.TryGetValue(item.UserId.Value, out var userName))
                {
                    item.UserName = userName;
                }
            }
        }

    }
}
