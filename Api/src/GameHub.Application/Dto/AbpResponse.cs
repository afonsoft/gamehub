using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
namespace GameHub.Dto;

/// <summary>
/// Standard ABP error response wrapper.
/// </summary>
public class AbpResponse
{
    /// <summary>Whether the request succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Error details (null on success).</summary>
    public AbpError Error { get; set; }

    /// <summary>Result payload (null on error).</summary>
    public object Result { get; set; }
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
    public object Details { get; set; }
}
