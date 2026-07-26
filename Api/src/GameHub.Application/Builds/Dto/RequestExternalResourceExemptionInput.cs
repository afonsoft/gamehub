using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Builds.Dto
{
    public class RequestExternalResourceExemptionInput
    {
        [Required]
        public Guid GameId { get; set; }

        [Required]
        [StringLength(256)]
        public string Domain { get; set; }

        [StringLength(128)]
        public string ProviderName { get; set; }

        [StringLength(512)]
        public string PrivacyStatementUrl { get; set; }
    }
}
