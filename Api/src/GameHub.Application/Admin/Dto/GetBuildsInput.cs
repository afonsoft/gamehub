using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Input for paginated build queries in the admin panel.
    /// </summary>
    public class GetBuildsInput
    {
        /// <summary>Number of items to skip.</summary>
        [Range(0, int.MaxValue)]
        public int SkipCount { get; set; }

        /// <summary>Maximum items per page.</summary>
        [Range(1, 100)]
        public int MaxResultCount { get; set; } = 24;

        /// <summary>Filter by build status.</summary>
        public string Status { get; set; }

        /// <summary>Optional filter by parent game identifier.</summary>
        public Guid? GameId { get; set; }

        /// <summary>Search by game title or developer display name.</summary>
        [StringLength(200)]
        public string SearchText { get; set; }
    }
}
