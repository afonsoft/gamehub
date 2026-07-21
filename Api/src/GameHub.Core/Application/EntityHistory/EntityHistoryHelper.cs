using Abp.Application.Features;
using Abp.Configuration;
using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.MultiTenancy;
using GameHub.Airplanes;
using System;
using System.Linq;

namespace GameHub.EntityHistory
{
    public static class EntityHistoryHelper
    {
        public static readonly Type[] ProjectNameTrackedTypes =
        {
            typeof(Role),
            typeof(Tenant),
            typeof(User),
            typeof(Setting),
            typeof(FeatureSetting),
            typeof(Airplane)
        };

        public static Type[] TrackedTypes { get; } = ProjectNameTrackedTypes
            .GroupBy(type => type.FullName)
            .Select(types => types.First())
            .ToArray();
    }
}