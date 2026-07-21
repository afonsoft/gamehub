# 14 — DTO Complete Reference

> **Status:** Draft  
> **Stack:** .NET 10 LTS · ASP.NET Boilerplate/EAF · ABP DTOs  
> **Conventions:** XML docs · DataAnnotations where needed · No FluentValidation

---

## 1. Authentication DTOs

### AuthenticateInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Authentication.Dto;

/// <summary>
/// Input for user authentication.
/// </summary>
public class AuthenticateInput
{
    /// <summary>User name or email address.</summary>
    [Required]
    [StringLength(256, MinimumLength = 3)]
    public string UserNameOrEmailAddress { get; set; } = string.Empty;

    /// <summary>User password.</summary>
    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
```

### AuthenticateOutput

```csharp
namespace GameHub.Authentication.Dto;

/// <summary>
/// Result of a successful authentication.
/// </summary>
public class AuthenticateOutput
{
    /// <summary>JWT access token.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Encrypted access token for cookie storage.</summary>
    public string EncryptedAccessToken { get; set; } = string.Empty;

    /// <summary>Token lifetime in seconds.</summary>
    public int ExpireInSeconds { get; set; }

    /// <summary>Authenticated user identifier.</summary>
    public long UserId { get; set; }
}
```

---

## 2. Catalog DTOs

### HomeResponseDto

```csharp
namespace GameHub.Catalog.Dto;

/// <summary>
/// Home page aggregated content.
/// </summary>
public class HomeResponseDto
{
    /// <summary>Featured/highlighted games curated by admin.</summary>
    public List<GameCardDto> Highlights { get; set; } = new();

    /// <summary>Recently published games.</summary>
    public List<GameCardDto> NewGames { get; set; } = new();

    /// <summary>Games with the most plays in the current period.</summary>
    public List<GameCardDto> MostPlayed { get; set; } = new();

    /// <summary>Games trending by recent play growth.</summary>
    public List<GameCardDto> Trending { get; set; } = new();

    /// <summary>All active categories for the sidebar/chips.</summary>
    public List<CategoryDto> Categories { get; set; } = new();
}
```

### GameCardDto

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Catalog.Dto;

/// <summary>
/// Compact game representation for cards and lists.
/// </summary>
public class GameCardDto
{
    /// <summary>Game unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Display title.</summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>URL-safe slug derived from title.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Thumbnail image URL.</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Short description for card display (max 160 chars).</summary>
    [StringLength(160)]
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Assigned categories.</summary>
    public List<CategoryDto> Categories { get; set; } = new();

    /// <summary>Whether the game supports mobile devices.</summary>
    public bool SupportsMobile { get; set; }

    /// <summary>Whether the game supports desktop browsers.</summary>
    public bool SupportsDesktop { get; set; }

    /// <summary>Total play count across all sessions.</summary>
    public long TotalPlays { get; set; }
}
```

### GameDetailDto

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Catalog.Dto;

/// <summary>
/// Full game representation for the detail page.
/// </summary>
public class GameDetailDto
{
    /// <summary>Game unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Thumbnail image URL.</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Short description for listings.</summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Full HTML/Markdown description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Game instructions (HTML/Markdown).</summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>Age rating (e.g., E, E10+, T, M).</summary>
    public string AgeRating { get; set; } = string.Empty;

    /// <summary>Game orientation: Portrait, Landscape, or Both.</summary>
    public string Orientation { get; set; } = string.Empty;

    /// <summary>Hero/banner image URL for detail page.</summary>
    public string HeroImageUrl { get; set; } = string.Empty;

    /// <summary>Developer display name.</summary>
    public string DeveloperName { get; set; } = string.Empty;

    /// <summary>URL to the published build's index.html.</summary>
    public string PublishedBuildUrl { get; set; } = string.Empty;

    /// <summary>Total play count.</summary>
    public long TotalPlays { get; set; }

    /// <summary>Average user rating (0–5).</summary>
    public decimal AverageRating { get; set; }

    /// <summary>Assigned tags.</summary>
    public List<TagDto> Tags { get; set; } = new();

    /// <summary>Related/similar games.</summary>
    public List<GameCardDto> RelatedGames { get; set; } = new();
}
```

### CategoryDto

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Catalog.Dto;

/// <summary>
/// Category lookup entry.
/// </summary>
public class CategoryDto
{
    /// <summary>Category unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Category display name.</summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Display sort order (ascending).</summary>
    public int SortOrder { get; set; }
}
```

### TagDto

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Catalog.Dto;

/// <summary>
/// Tag lookup entry.
/// </summary>
public class TagDto
{
    /// <summary>Tag unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Tag display name.</summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;
}
```

### GetGamesInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Catalog.Dto;

/// <summary>
/// Input for paginated game catalog queries.
/// </summary>
public class GetGamesInput
{
    /// <summary>Number of items to skip.</summary>
    [Range(0, int.MaxValue)]
    public int SkipCount { get; set; }

    /// <summary>Maximum items per page.</summary>
    [Range(1, 100)]
    public int MaxResultCount { get; set; } = 24;

    /// <summary>Sort field: "Newest", "MostPlayed", "TopRated", "Title".</summary>
    public string Sorting { get; set; } = "Newest";

    /// <summary>Filter by category slug.</summary>
    public string? CategorySlug { get; set; }

    /// <summary>Filter by tag slug.</summary>
    public string? TagSlug { get; set; }

    /// <summary>Filter by device: "Desktop", "Mobile", "Tablet".</summary>
    public string? Device { get; set; }

    /// <summary>Filter by orientation: "Portrait", "Landscape".</summary>
    public string? Orientation { get; set; }
}
```

### SearchInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Catalog.Dto;

/// <summary>
/// Input for full-text game search with filters.
/// </summary>
public class SearchInput
{
    /// <summary>Search query string.</summary>
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Query { get; set; } = string.Empty;

    /// <summary>Filter by category slugs.</summary>
    public List<string>? Categories { get; set; }

    /// <summary>Filter by tag slugs.</summary>
    public List<string>? Tags { get; set; }

    /// <summary>Filter by device.</summary>
    public string? Device { get; set; }

    /// <summary>Filter by orientation.</summary>
    public string? Orientation { get; set; }

    /// <summary>Number of items to skip.</summary>
    [Range(0, int.MaxValue)]
    public int SkipCount { get; set; }

    /// <summary>Maximum items per page.</summary>
    [Range(1, 100)]
    public int MaxResultCount { get; set; } = 24;

    /// <summary>Sort field.</summary>
    public string Sorting { get; set; } = "Relevance";
}
```

### PagedResultDto\<T\>

```csharp
namespace GameHub.Catalog.Dto;

/// <summary>
/// Generic paged result wrapper.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public class PagedResultDto<T>
{
    /// <summary>Total number of matching items.</summary>
    public long TotalCount { get; set; }

    /// <summary>Items for the current page.</summary>
    public List<T> Items { get; set; } = new();
}
```

---

## 3. Gameplay DTOs

### StartPlaySessionInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Gameplay.Dto;

/// <summary>
/// Input to start a new play session.
/// </summary>
public class StartPlaySessionInput
{
    /// <summary>Game to play.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Player device type: "Desktop", "Mobile", "Tablet".</summary>
    [Required]
    [StringLength(20)]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>Browser user agent string.</summary>
    [StringLength(500)]
    public string? Browser { get; set; }

    /// <summary>HTTP referrer that led to the game.</summary>
    [StringLength(500)]
    public string? Referrer { get; set; }
}
```

### PlaySessionDto

```csharp
namespace GameHub.Gameplay.Dto;

/// <summary>
/// Active play session metadata.
/// </summary>
public class PlaySessionDto
{
    /// <summary>Session unique identifier.</summary>
    public Guid SessionId { get; set; }

    /// <summary>Game being played.</summary>
    public Guid GameId { get; set; }

    /// <summary>UTC timestamp when the session started.</summary>
    public DateTime StartedAt { get; set; }
}
```

### GameplayEventInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Gameplay.Dto;

/// <summary>
/// A single gameplay event from the Game SDK.
/// </summary>
public class GameplayEventInput
{
    /// <summary>Session identifier.</summary>
    [Required]
    public Guid SessionId { get; set; }

    /// <summary>Type of gameplay event.</summary>
    [Required]
    public GameplayEventType EventType { get; set; }

    /// <summary>Event name (e.g., "level_complete").</summary>
    [StringLength(100)]
    public string? EventName { get; set; }

    /// <summary>Arbitrary JSON payload.</summary>
    [StringLength(4096)]
    public string? PayloadJson { get; set; }
}

/// <summary>
/// Supported gameplay event types.
/// </summary>
public enum GameplayEventType
{
    /// <summary>Game SDK finished loading.</summary>
    GameLoadingFinished = 0,

    /// <summary>User started gameplay.</summary>
    GameplayStart = 1,

    /// <summary>User stopped gameplay.</summary>
    GameplayStop = 2,

    /// <summary>Commercial/ad break.</summary>
    CommercialBreak = 3,

    /// <summary>Rewarded ad break.</summary>
    RewardedBreak = 4,

    /// <summary>Game reported an error.</summary>
    CaptureError = 5,

    /// <summary>Performance measurement.</summary>
    Measure = 6
}
```

### SubmitScoreInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Gameplay.Dto;

/// <summary>
/// Input to submit a score to the leaderboard.
/// </summary>
public class SubmitScoreInput
{
    /// <summary>Game identifier.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Score value (higher is better).</summary>
    [Required]
    [Range(0, long.MaxValue)]
    public long Score { get; set; }

    /// <summary>Optional metadata JSON (level, combo, etc.).</summary>
    [StringLength(4096)]
    public string? MetadataJson { get; set; }
}
```

### LeaderboardEntryDto

```csharp
namespace GameHub.Gameplay.Dto;

/// <summary>
/// Single leaderboard entry.
/// </summary>
public class LeaderboardEntryDto
{
    /// <summary>Rank position (1-based).</summary>
    public int Rank { get; set; }

    /// <summary>User identifier.</summary>
    public long UserId { get; set; }

    /// <summary>Player display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Best score achieved.</summary>
    public long Score { get; set; }

    /// <summary>UTC timestamp of the score update.</summary>
    public DateTime UpdatedAt { get; set; }
}
```

### GetLeaderboardInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Gameplay.Dto;

/// <summary>
/// Input for leaderboard queries.
/// </summary>
public class GetLeaderboardInput
{
    /// <summary>Game identifier.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Number of top entries to return.</summary>
    [Range(1, 100)]
    public int Take { get; set; } = 50;
}
```

---

## 4. Developer DTOs

### DeveloperProfileDto

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto;

/// <summary>
/// Developer profile data.
/// </summary>
public class DeveloperProfileDto
{
    /// <summary>Profile unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Public display name.</summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Legal company name (for invoices).</summary>
    [StringLength(200)]
    public string? LegalName { get; set; }

    /// <summary>Developer website URL.</summary>
    [Url]
    [StringLength(500)]
    public string? WebsiteUrl { get; set; }

    /// <summary>Support email for players.</summary>
    [EmailAddress]
    [StringLength(256)]
    public string? SupportEmail { get; set; }

    /// <summary>Profile status: "Pending", "Active", "Suspended".</summary>
    public string Status { get; set; } = "Pending";
}
```

### CreateOrUpdateDeveloperProfileInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto;

/// <summary>
/// Input to create or update a developer profile.
/// </summary>
public class CreateOrUpdateDeveloperProfileInput
{
    /// <summary>Public display name.</summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Legal company name.</summary>
    [StringLength(200)]
    public string? LegalName { get; set; }

    /// <summary>Developer website URL.</summary>
    [Url]
    [StringLength(500)]
    public string? WebsiteUrl { get; set; }

    /// <summary>Support email for players.</summary>
    [EmailAddress]
    [StringLength(256)]
    public string? SupportEmail { get; set; }
}
```

### GameSummaryDto

```csharp
namespace GameHub.Developer.Dto;

/// <summary>
/// Developer-facing game summary for the game list.
/// </summary>
public class GameSummaryDto
{
    /// <summary>Game unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Game status: "Draft", "InReview", "Published", "Suspended".</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Thumbnail image URL.</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Currently published build version string.</summary>
    public string? PublishedBuildVersion { get; set; }

    /// <summary>Total play count.</summary>
    public long TotalPlays { get; set; }

    /// <summary>UTC timestamp of the last metadata update.</summary>
    public DateTime LastUpdated { get; set; }
}
```

### CreateGameDraftInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto;

/// <summary>
/// Input to create a new game draft.
/// </summary>
public class CreateGameDraftInput
{
    /// <summary>Game title.</summary>
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Short description for listings (max 200 chars).</summary>
    [Required]
    [StringLength(200, MinimumLength = 10)]
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Full description (HTML/Markdown).</summary>
    [Required]
    [StringLength(50000, MinimumLength = 20)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Game instructions (HTML/Markdown).</summary>
    [StringLength(10000)]
    public string? Instructions { get; set; }

    /// <summary>Age rating: "E", "E10+", "T", "M".</summary>
    [Required]
    [StringLength(10)]
    public string AgeRating { get; set; } = "E";

    /// <summary>Orientation: "Portrait", "Landscape", "Both".</summary>
    [Required]
    [StringLength(20)]
    public string Orientation { get; set; } = "Both";

    /// <summary>Supports desktop browsers.</summary>
    public bool SupportsDesktop { get; set; } = true;

    /// <summary>Supports mobile devices.</summary>
    public bool SupportsMobile { get; set; }

    /// <summary>Supports tablet devices.</summary>
    public bool SupportsTablet { get; set; }

    /// <summary>Category identifiers to assign.</summary>
    public List<Guid>? CategoryIds { get; set; }

    /// <summary>Tag identifiers to assign.</summary>
    public List<Guid>? TagIds { get; set; }
}
```

### UpdateGameMetadataInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto;

/// <summary>
/// Input to update game metadata.
/// </summary>
public class UpdateGameMetadataInput
{
    /// <summary>Game to update.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Game title.</summary>
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Short description for listings.</summary>
    [Required]
    [StringLength(200, MinimumLength = 10)]
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Full description.</summary>
    [Required]
    [StringLength(50000, MinimumLength = 20)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Game instructions.</summary>
    [StringLength(10000)]
    public string? Instructions { get; set; }

    /// <summary>Age rating.</summary>
    [Required]
    [StringLength(10)]
    public string AgeRating { get; set; } = "E";

    /// <summary>Orientation.</summary>
    [Required]
    [StringLength(20)]
    public string Orientation { get; set; } = "Both";

    /// <summary>Supports desktop browsers.</summary>
    public bool SupportsDesktop { get; set; } = true;

    /// <summary>Supports mobile devices.</summary>
    public bool SupportsMobile { get; set; }

    /// <summary>Supports tablet devices.</summary>
    public bool SupportsTablet { get; set; }

    /// <summary>Category identifiers to assign.</summary>
    public List<Guid>? CategoryIds { get; set; }

    /// <summary>Tag identifiers to assign.</summary>
    public List<Guid>? TagIds { get; set; }
}
```

### SubmitGameForReviewInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto;

/// <summary>
/// Input to submit a game draft for moderation review.
/// </summary>
public class SubmitGameForReviewInput
{
    /// <summary>Game identifier.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Optional notes for the reviewer.</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}
```

### BuildDto

```csharp
namespace GameHub.Developer.Dto;

/// <summary>
/// Build metadata.
/// </summary>
public class BuildDto
{
    /// <summary>Build unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Version string (e.g., "1.0.0").</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Monotonically increasing build number.</summary>
    public int BuildNumber { get; set; }

    /// <summary>Build status: "Uploading", "Validating", "Valid", "Invalid", "Published".</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Build zip size in bytes.</summary>
    public long SizeBytes { get; set; }

    /// <summary>SHA-256 hash of the build zip.</summary>
    public string HashSha256 { get; set; } = string.Empty;

    /// <summary>Validation result summary (JSON or structured).</summary>
    public string? ValidationSummary { get; set; }

    /// <summary>UTC timestamp when the build was uploaded.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp when the build was published (null if not published).</summary>
    public DateTime? PublishedAt { get; set; }
}
```

### UploadGameBuildResultDto

```csharp
namespace GameHub.Developer.Dto;

/// <summary>
/// Result of a build upload and validation.
/// </summary>
public class UploadGameBuildResultDto
{
    /// <summary>Build unique identifier.</summary>
    public Guid BuildId { get; set; }

    /// <summary>Build version string.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Build status after validation.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Validation result summary.</summary>
    public string? ValidationSummary { get; set; }
}
```

---

## 5. Admin DTOs

### AdminGameListItemDto

```csharp
namespace GameHub.Admin.Dto;

/// <summary>
/// Game list item for the admin panel.
/// </summary>
public class AdminGameListItemDto
{
    /// <summary>Game unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Developer display name.</summary>
    public string DeveloperName { get; set; } = string.Empty;

    /// <summary>Game status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Thumbnail image URL.</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Total play count.</summary>
    public long TotalPlays { get; set; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }
}
```

### AdminGameDetailDto

```csharp
namespace GameHub.Admin.Dto;

/// <summary>
/// Full game detail for the admin panel, including build and moderation history.
/// </summary>
public class AdminGameDetailDto
{
    /// <summary>Game unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Short description.</summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Full description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Game instructions.</summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>Age rating.</summary>
    public string AgeRating { get; set; } = string.Empty;

    /// <summary>Orientation.</summary>
    public string Orientation { get; set; } = string.Empty;

    /// <summary>Thumbnail image URL.</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Hero/banner image URL.</summary>
    public string HeroImageUrl { get; set; } = string.Empty;

    /// <summary>Developer display name.</summary>
    public string DeveloperName { get; set; } = string.Empty;

    /// <summary>Game status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Total play count.</summary>
    public long TotalPlays { get; set; }

    /// <summary>Average rating.</summary>
    public decimal AverageRating { get; set; }

    /// <summary>Full build history.</summary>
    public List<BuildDto> BuildHistory { get; set; } = new();

    /// <summary>Full moderation review history.</summary>
    public List<ModerationReviewDto> ModerationHistory { get; set; } = new();

    /// <summary>Assigned categories.</summary>
    public List<CategoryDto> Categories { get; set; } = new();

    /// <summary>Assigned tags.</summary>
    public List<TagDto> Tags { get; set; } = new();

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }
}
```

### ApproveBuildInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto;

/// <summary>
/// Input to approve a game build after moderation review.
/// </summary>
public class ApproveBuildInput
{
    /// <summary>Build to approve.</summary>
    [Required]
    public Guid GameBuildId { get; set; }

    /// <summary>Optional approval notes.</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}
```

### RejectBuildInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto;

/// <summary>
/// Input to reject a game build after moderation review.
/// </summary>
public class RejectBuildInput
{
    /// <summary>Build to reject.</summary>
    [Required]
    public Guid GameBuildId { get; set; }

    /// <summary>Rejection reason (required).</summary>
    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}
```

### PublishGameInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto;

/// <summary>
/// Input to publish a game to production.
/// </summary>
public class PublishGameInput
{
    /// <summary>Game to publish.</summary>
    [Required]
    public Guid GameId { get; set; }
}
```

### SuspendGameInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto;

/// <summary>
/// Input to suspend a live game.
/// </summary>
public class SuspendGameInput
{
    /// <summary>Game to suspend.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Reason for suspension (visible to developer).</summary>
    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}
```

### ModerationReviewDto

```csharp
namespace GameHub.Admin.Dto;

/// <summary>
/// Moderation review record.
/// </summary>
public class ModerationReviewDto
{
    /// <summary>Review unique identifier.</summary>
    public Guid ReviewId { get; set; }

    /// <summary>Game being reviewed.</summary>
    public Guid GameId { get; set; }

    /// <summary>Game title.</summary>
    public string GameTitle { get; set; } = string.Empty;

    /// <summary>Build being reviewed.</summary>
    public Guid GameBuildId { get; set; }

    /// <summary>Reviewer display name.</summary>
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>Review status: "Pending", "InProgress", "Completed".</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Review decision: "Approved", "Rejected", "RequiresChanges".</summary>
    public string? Decision { get; set; }

    /// <summary>Reviewer notes.</summary>
    public string? Notes { get; set; }

    /// <summary>UTC timestamp when the review was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp when the review was completed (null if pending).</summary>
    public DateTime? CompletedAt { get; set; }
}
```

### CompleteReviewInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto;

/// <summary>
/// Input to complete a moderation review.
/// </summary>
public class CompleteReviewInput
{
    /// <summary>Review to complete.</summary>
    [Required]
    public Guid ReviewId { get; set; }

    /// <summary>Review decision.</summary>
    [Required]
    public ReviewDecision Decision { get; set; }

    /// <summary>Reviewer notes (required for rejections).</summary>
    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Possible moderation review decisions.
/// </summary>
public enum ReviewDecision
{
    /// <summary>Build is approved and can be published.</summary>
    Approved = 0,

    /// <summary>Build is rejected and cannot be published.</summary>
    Rejected = 1,

    /// <summary>Build requires changes before resubmission.</summary>
    RequiresChanges = 2
}
```

### UserReportDto

```csharp
namespace GameHub.Admin.Dto;

/// <summary>
/// User-submitted report about a game.
/// </summary>
public class UserReportDto
{
    /// <summary>Report unique identifier.</summary>
    public Guid ReportId { get; set; }

    /// <summary>Game being reported.</summary>
    public Guid GameId { get; set; }

    /// <summary>Game title.</summary>
    public string GameTitle { get; set; } = string.Empty;

    /// <summary>User who submitted the report.</summary>
    public long UserId { get; set; }

    /// <summary>Report reason category.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Free-text description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Report status: "Open", "UnderReview", "Resolved", "Dismissed".</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the report was created.</summary>
    public DateTime CreatedAt { get; set; }
}
```

### CreateOrUpdateCategoryInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto;

/// <summary>
/// Input to create or update a category.
/// </summary>
public class CreateOrUpdateCategoryInput
{
    /// <summary>Category identifier (null for creation).</summary>
    public Guid? Id { get; set; }

    /// <summary>Category display name.</summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-safe slug (auto-generated if empty).</summary>
    [StringLength(100)]
    public string? Slug { get; set; }

    /// <summary>Display sort order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Whether the category is active and visible.</summary>
    public bool IsActive { get; set; } = true;
}
```

### CreateOrUpdateTagInput

```csharp
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto;

/// <summary>
/// Input to create or update a tag.
/// </summary>
public class CreateOrUpdateTagInput
{
    /// <summary>Tag identifier (null for creation).</summary>
    public Guid? Id { get; set; }

    /// <summary>Tag display name.</summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-safe slug (auto-generated if empty).</summary>
    [StringLength(100)]
    public string? Slug { get; set; }
}
```

---

## 6. Error Response

### AbpResponse

```csharp
namespace GameHub.Dto;

/// <summary>
/// Standard ABP error response wrapper.
/// </summary>
public class AbpResponse
{
    /// <summary>Whether the request succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Error details (null on success).</summary>
    public AbpError? Error { get; set; }

    /// <summary>Result payload (null on error).</summary>
    public object? Result { get; set; }
}

/// <summary>
/// Structured error information.
/// </summary>
public class AbpError
{
    /// <summary>Error code for programmatic handling.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>User-friendly error message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Detailed validation errors or inner exceptions.</summary>
    public object? Details { get; set; }
}
```

### Example error response

```json
{
  "success": false,
  "error": {
    "code": "GameHub.ValidationError",
    "message": "Validation failed.",
    "details": [
      {
        "field": "Title",
        "message": "Title is required."
      },
      {
        "field": "AgeRating",
        "message": "AgeRating must be one of: E, E10+, T, M."
      }
    ]
  },
  "result": null
}
```

### Example success response

```json
{
  "success": true,
  "error": null,
  "result": {
    "totalCount": 142,
    "items": [
      {
        "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "title": "Space Explorer",
        "slug": "space-explorer",
        "thumbnailUrl": "https://gamehub.afonsoft.dev/media/games/space-explorer/thumb.png",
        "shortDescription": "Explore the galaxy in this adventure game.",
        "totalPlays": 12450
      }
    ]
  }
}
```
