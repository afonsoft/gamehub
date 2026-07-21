using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
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
