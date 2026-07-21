using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
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
