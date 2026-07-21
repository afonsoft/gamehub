using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using GameHub.Admin.Dto;
using GameHub.Moderation;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Admin
{
    /// <summary>
    /// Gestão de reports de usuários no painel administrativo.
    /// </summary>
    public class AdminReportAppService : ApplicationService, IAdminReportAppService
    {
        private readonly IRepository<UserReport, Guid> _userReportRepository;

        public AdminReportAppService(IRepository<UserReport, Guid> userReportRepository)
        {
            _userReportRepository = userReportRepository;
        }

        public async Task<PagedResultDto<UserReportDto>> GetAllAsync(GetReportsInput input)
        {
            var query = _userReportRepository.GetAll().Where(r => !r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(input.Status))
            {
                if (Enum.TryParse<UserReportStatus>(input.Status, true, out var status))
                {
                    query = query.Where(r => r.Status == status);
                }
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<UserReportDto>(total, ObjectMapper.Map<List<UserReportDto>>(items));
        }

        public async Task UpdateStatusAsync(Guid reportId, string status)
        {
            if (!Enum.TryParse<UserReportStatus>(status, true, out var parsedStatus))
            {
                throw new System.ComponentModel.DataAnnotations.ValidationException($"Status inválido: {status}");
            }

            var report = await _userReportRepository.GetAsync(reportId);
            report.Status = parsedStatus;

            if (parsedStatus == UserReportStatus.Resolved || parsedStatus == UserReportStatus.Dismissed)
            {
                report.ResolvedAt = DateTime.UtcNow;
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
