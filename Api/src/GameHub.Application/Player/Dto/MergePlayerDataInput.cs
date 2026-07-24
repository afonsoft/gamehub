using System;
using System.Collections.Generic;

namespace GameHub.Player.Dto
{
    /// <summary>
    /// Local favorites and recent games to merge into the authenticated account.
    /// </summary>
    public class MergePlayerDataInput
    {
        public List<Guid> FavoriteGameIds { get; set; } = new();

        public List<Guid> RecentGameIds { get; set; } = new();
    }
}
