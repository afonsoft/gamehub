using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.ArbitraryUserData.Dto
{
    public class GetArbitraryUserDataInput
    {
        [Required]
        public Guid GameId { get; set; }

        [StringLength(128)]
        public string AnonymousIdHash { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string Key { get; set; } = string.Empty;
    }

    public class SetArbitraryUserDataInput
    {
        [Required]
        public Guid GameId { get; set; }

        [StringLength(128)]
        public string AnonymousIdHash { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string Key { get; set; } = string.Empty;

        public string ValueJson { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int? TtlSeconds { get; set; }
    }

    public class DeleteArbitraryUserDataInput
    {
        [Required]
        public Guid GameId { get; set; }

        [StringLength(128)]
        public string AnonymousIdHash { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string Key { get; set; } = string.Empty;
    }

    public class ArbitraryUserDataQuotaDto
    {
        public Guid GameId { get; set; }

        public long TotalKeys { get; set; }

        public long TotalBytes { get; set; }

        public long MaxKeys { get; set; }

        public long MaxBytesPerValue { get; set; }
    }
}
