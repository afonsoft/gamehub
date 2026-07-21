using Xunit;

namespace GameHub.Tests
{
    public sealed class MultiTenantFactAttribute : FactAttribute
    {
        public MultiTenantFactAttribute()
        {
            Skip = ProjectNameConsts.MultiTenancyEnabled ? null : "MultiTenancy is disabled.";
        }
    }
}