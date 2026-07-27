namespace GameHub;

/// <summary>
/// Stable error codes exposed to SDK consumers.
/// </summary>
public static class GameHubErrorCodes
{
    public const string NotAuthenticated = "not_authenticated";
    public const string NotAuthorized = "not_authorized";
    public const string FeatureDisabled = "feature_disabled";
    public const string RateLimited = "rate_limited";
    public const string InvalidContext = "invalid_context";
    public const string TemporarilyUnavailable = "temporarily_unavailable";
    public const string ValidationFailed = "validation_failed";
}
