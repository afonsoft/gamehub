using GameHub.Moderation;
using GameHub.Gameplay;
using GameHub.Developers;
using GameHub.Catalog;
using GameHub.Builds;
using GameHub;
using System;

namespace GameHub
{
    public enum GameStatus
    {
        Draft = 0,
        InReview = 1,
        Published = 2,
        Rejected = 3,
        Suspended = 4,
        Archived = 5,
        Submitted = 6,
        ApprovedForPublishing = 7
    }

    public enum GameBuildStatus
    {
        Uploaded = 0,
        Validating = 1,
        Validated = 2,
        ValidationFailed = 3,
        InReview = 4,
        Approved = 5,
        Published = 6,
        Rejected = 7,
        Blocked = 8
    }

    public enum GameOrientation
    {
        Landscape = 0,
        Portrait = 1,
        Both = 2
    }

    public enum GameplayEventType
    {
        GameLoadingStarted = 0,
        GameLoadingFinished = 1,
        GameplayStarted = 2,
        GameplayStopped = 3,
        CommercialBreakRequested = 4,
        CommercialBreakCompleted = 5,
        RewardedBreakRequested = 6,
        RewardedBreakCompleted = 7,
        GameErrorCaptured = 8,
        GameMeasuredEvent = 9,
        GamePageViewed = 10
    }

    public enum GamePlacementType
    {
        Featured = 0,
        Trending = 1,
        NewCategory = 2,
        Carousel = 3
    }

    public enum DeveloperProfileStatus
    {
        Pending = 0,
        Active = 1,
        Suspended = 2,
        Banned = 3
    }

    public enum ModerationReviewStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2
    }

    public enum ModerationDecision
    {
        Approved = 0,
        Rejected = 1,
        RequiresChanges = 2
    }

    public enum UserReportStatus
    {
        Open = 0,
        UnderReview = 1,
        Resolved = 2,
        Dismissed = 3
    }
}
