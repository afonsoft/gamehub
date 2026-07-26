using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.ArbitraryUserData;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Jobs
{
    /// <summary>
    /// Removes expired arbitrary user data records.
    /// </summary>
    public class CleanupExpiredArbitraryUserDataJob : ITransientDependency
    {
        private readonly IRepository<ArbitraryUserDataRecord, Guid> _repository;

        public CleanupExpiredArbitraryUserDataJob(IRepository<ArbitraryUserDataRecord, Guid> repository)
        {
            _repository = repository;
        }

        public async Task Execute()
        {
            var expired = await _repository.GetAll()
                .Where(item => item.ExpiresAt.HasValue && item.ExpiresAt <= Clock.Now)
                .ToListAsync();

            foreach (var item in expired)
            {
                await _repository.DeleteAsync(item);
            }
        }
    }
}
