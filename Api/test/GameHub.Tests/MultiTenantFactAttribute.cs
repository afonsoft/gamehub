using Xunit;

namespace GameHub.Tests
{
    public sealed class MultiTenantFactAttribute : FactAttribute
    {
        public MultiTenantFactAttribute()
        {
            Skip = GameHubConsts.MultiTenancyEnabled ? null : "MultiTenancy is disabled.";
        }
    }
}