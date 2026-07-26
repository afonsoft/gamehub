using System;

namespace GameHub.Builds.Dto
{
    public class ExternalResourceExemptionDto
    {
        public Guid Id { get; set; }

        public Guid GameId { get; set; }

        public string Domain { get; set; }

        public string ProviderName { get; set; }

        public string PrivacyStatementUrl { get; set; }

        public string Status { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string ModeratorNotes { get; set; }
    }
}
