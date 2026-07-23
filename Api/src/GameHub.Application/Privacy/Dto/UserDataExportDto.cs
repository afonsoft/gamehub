using System;
using System.Collections.Generic;

namespace GameHub.Privacy.Dto
{
    public class UserDataExportDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public List<PlaySessionExportDto> PlaySessions { get; set; } = new();
        public List<LeaderboardEntryExportDto> LeaderboardEntries { get; set; } = new();
        public List<UserReportExportDto> UserReports { get; set; } = new();
        public DeveloperProfileExportDto DeveloperProfile { get; set; }
    }

    public class PlaySessionExportDto
    {
        public Guid SessionId { get; set; }
        public Guid GameId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string DeviceType { get; set; }
        public string Browser { get; set; }
        public string CountryCode { get; set; }
    }

    public class LeaderboardEntryExportDto
    {
        public Guid GameId { get; set; }
        public long Score { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserReportExportDto
    {
        public Guid ReportId { get; set; }
        public Guid GameId { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class DeveloperProfileExportDto
    {
        public string DisplayName { get; set; }
        public string LegalName { get; set; }
        public string WebsiteUrl { get; set; }
        public string SupportEmail { get; set; }
    }
}
