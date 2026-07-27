using System;

namespace GameHub.Exceptions;

/// <summary>
/// Domain/application exception that carries a public SDK error code.
/// </summary>
public class GameHubException : Exception
{
    /// <summary>
    /// Stable error code for programmatic handling.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Whether the client may retry the operation.
    /// </summary>
    public bool Retryable { get; }

    public GameHubException(string errorCode, string message, bool retryable = false, Exception innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Retryable = retryable;
    }
}
