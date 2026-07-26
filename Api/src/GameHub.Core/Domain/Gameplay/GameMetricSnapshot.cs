using GameHub.Moderation;
using GameHub.Gameplay;
using GameHub.Developers;
using GameHub.Catalog;
using GameHub.Builds;
using GameHub;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Gameplay
{
    public class GameMetricSnapshot : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public GameMetricSnapshot() { }

        public Guid GameId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public long Plays { get; set; }
        [Required]
        public long UniquePlayers { get; set; }
        [Required]
        public double AvgDurationSeconds { get; set; }
        [Required]
        public long LoadingFinishedCount { get; set; }
        [Required]
        public long ErrorCount { get; set; }
        [Required]
        public long CommercialBreakCount { get; set; }
        [Required]
        public long RewardedBreakCount { get; set; }

        /// <summary>Average session duration in seconds for this date.</summary>
        public double AvgSessionDurationSeconds { get; set; }

        /// <summary>Median session duration in seconds for this date.</summary>
        public double MedianSessionDurationSeconds { get; set; }

        /// <summary>Percentage of sessions that ended within the first 60 seconds.</summary>
        public double OnboardingDropOffRate { get; set; }

        /// <summary>Average FPS aggregated across sessions for this date.</summary>
        public double? AvgFps { get; set; }

        /// <summary>Minimum FPS recorded for this date.</summary>
        public double? MinFps { get; set; }

        /// <summary>Number of sessions with acceptable FPS (>= 30).</summary>
        public long FpsAcceptableSessions { get; set; }

        /// <summary>Total number of sessions that reported FPS.</summary>
        public long FpsTotalSessions { get; set; }

        public virtual Game Game { get; set; }
    }
}