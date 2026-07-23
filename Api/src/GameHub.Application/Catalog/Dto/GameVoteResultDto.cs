using System;
using GameHub.Catalog;

namespace GameHub.Catalog.Dto
{
    /// <summary>
    /// Result of a vote operation, including updated counters and the current user's vote.
    /// </summary>
    public class GameVoteResultDto
    {
        public Guid GameId { get; set; }

        public long TotalLikes { get; set; }

        public long TotalDislikes { get; set; }

        public GameVoteType? UserVote { get; set; }
    }
}
