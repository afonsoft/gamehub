---
name: security-jwt
license: UNLICENSED
description: Use when implementing JWT authentication in .NET applications, configuring authorization policies, or securing API endpoints
---

# JWT Security Skill

## When to Use This Skill

Use this skill when you need to:
- Implement JWT-based authentication in .NET applications
- Configure secure authorization policies and roles
- Handle token generation, validation, and refresh
- Implement proper security middleware and filters
- Apply security best practices and prevent common vulnerabilities
- Set up secure API endpoints with proper authorization

## Prerequisites

- .NET 6+ ASP.NET Core application
- Understanding of authentication and authorization concepts
- Familiarity with JWT tokens and claims
- Knowledge of ASP.NET Core middleware pipeline
- Basic understanding of security best practices

## Step-by-Step Workflows

### 1. JWT Configuration Setup

**Input**: Application requirements and security policies  
**Output: Complete JWT authentication configuration

```
Configure JWT authentication with these requirements:

1. Set up JWT token generation and validation
2. Configure authentication middleware
3. Define security policies and requirements
4. Set up token refresh mechanism
5. Configure secure cookie options
6. Add proper logging and error handling
7. Test authentication flow end-to-end
```

### 2. Authorization Implementation

**Input**: User roles and access control requirements  
**Output: Complete authorization system with policies

```
Implement authorization with these requirements:

1. Define role-based access control
2. Create custom authorization policies
3. Implement policy-based authorization
4. Add resource-based authorization
5. Configure authorization middleware
6. Test authorization scenarios
7. Handle unauthorized access properly
```

### 3. Security Middleware Setup

**Input**: Security requirements and threat model  
**Output: Complete security middleware pipeline

```
Set up security middleware with these requirements:

1. Configure authentication and authorization order
2. Add CORS policies for cross-origin requests
3. Implement rate limiting and throttling
4. Add security headers middleware
5. Configure HTTPS enforcement
6. Add request logging and auditing
7. Test security pipeline effectiveness
```

## JWT Configuration

### JWT Service Implementation

```csharp
// Interfaces
public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateToken(string token);
    TokenValidationParameters GetTokenValidationParameters();
}

public interface ITokenService
{
    Task<AuthenticationResult> AuthenticateAsync(LoginRequest request);
    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string token);
}

// JWT Service Implementation
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    private readonly SymmetricSecurityKey _key;
    
    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
    }
    
    public string GenerateToken(User user)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("username", user.Username),
                new Claim("permissions", string.Join(",", user.Permissions)),
                new Claim("last_password_change", user.LastPasswordChange?.ToString("O") ?? ""),
                new Claim("jti", Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60)),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature)
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        _logger.LogInformation("JWT token generated for user {UserId}", user.Id);
        return tokenHandler.WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = GetTokenValidationParameters();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogWarning("Invalid token algorithm");
            return null;
        }
        
        return principal;
    }
    
    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetTokenValidationParameters();
            
            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            
            return validatedToken != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return false;
        }
    }
    
    public TokenValidationParameters GetTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = _key,
            ClockSkew = TimeSpan.Zero
        };
    }
}
```

### Token Service Implementation

```csharp
public class TokenService : ITokenService
{
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<TokenService> _logger;
    
    public TokenService(
        IJwtService jwtService,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<TokenService> logger)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }
    
    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        try
        {
            // Validate user credentials
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Authentication failed for user {Username}", request.Username);
                return AuthenticationResult.Failed("Invalid username or password");
            }
            
            // Check if account is locked
            if (user.IsLockedOut)
            {
                _logger.LogWarning("Account locked for user {Username}", request.Username);
                return AuthenticationResult.Failed("Account is locked");
            }
            
            // Check if password needs to be changed
            if (user.MustChangePassword)
            {
                return new AuthenticationResult
                {
                    Success = true,
                    RequiresPasswordChange = true,
                    Message = "Password change required"
                };
            }
            
            // Generate tokens
            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            
            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = GetClientIpAddress()
            };
            
            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            
            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation("User {Username} authenticated successfully", request.Username);
            
            return new AuthenticationResult
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role,
                    Permissions = user.Permissions
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed for user {Username}", request.Username);
            return AuthenticationResult.Failed("Authentication failed");
        }
    }
    
    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Validate refresh token
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token");
                return AuthenticationResult.Failed("Invalid refresh token");
            }
            
            // Get user
            var user = await _userRepository.GetByIdAsync(storedToken.UserId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("User not found or inactive for refresh token");
                return AuthenticationResult.Failed("User not found or inactive");
            }
            
            // Generate new tokens
            var newToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            
            // Revoke old refresh token
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(storedToken);
            
            // Save new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = GetClientIpAddress(),
                ReplacedByToken = storedToken.Token
            };
            
            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
            
            _logger.LogInformation("Token refreshed for user {UserId}", user.Id);
            
            return new AuthenticationResult
            {
                Success = true,
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role,
                    Permissions = user.Permissions
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return AuthenticationResult.Failed("Token refresh failed");
        }
    }
    
    public async Task RevokeTokenAsync(string token)
    {
        try
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(token);
            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;
                await _refreshTokenRepository.UpdateAsync(storedToken);
                
                _logger.LogInformation("Token revoked for user {UserId}", storedToken.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token revocation failed");
        }
    }
    
    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
    
    private string GetClientIpAddress()
    {
        // Get client IP address from HttpContext
        // This would typically be injected via IHttpContextAccessor
        return "127.0.0.1";
    }
}
```

## Authentication Configuration

### Program.cs Configuration

```csharp
// JWT Authentication Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogDebug("Token validated for user {UserId}", 
                    context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Authentication challenge: {Error}", context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });

// Authorization Configuration
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
    
    // Custom policies
    options.AddPolicy("CanManageUsers", policy => 
        policy.RequireClaim("permissions", "users.manage"));
    
    options.AddPolicy("CanViewReports", policy => 
        policy.RequireClaim("permissions", "reports.view"));
    
    // Age-based policy
    options.AddPolicy("AtLeast18", policy => 
        policy.RequireClaim("age", x => Convert.ToInt32(x) >= 18));
    
    // Multi-claim policy
    options.AddPolicy("PremiumUser", policy => 
        policy.RequireRole("Premium")
              .RequireClaim("subscription", "active"));
    
    // Custom requirement policy
    options.AddPolicy("AccountActive", policy => 
        policy.AddRequirements(new AccountActiveRequirement()));
});

// Register custom policy handler
builder.Services.AddSingleton<IAuthorizationHandler, AccountActiveHandler>();

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
```

### Custom Authorization Requirements

```csharp
// Custom requirement
public class AccountActiveRequirement : IAuthorizationRequirement
{
}

// Requirement handler
public class AccountActiveHandler : AuthorizationHandler<AccountActiveRequirement>
{
    private readonly IUserRepository _userRepository;
    
    public AccountActiveHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccountActiveRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            context.Fail();
            return;
        }
        
        var userId = int.Parse(userIdClaim.Value);
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null || !user.IsActive || user.IsLockedOut)
        {
            context.Fail();
            return;
        }
        
        context.Succeed(requirement);
    }
}

// Resource-based requirement
public class SameUserRequirement : IAuthorizationRequirement
{
    public int UserId { get; }
    
    public SameUserRequirement(int userId)
    {
        UserId = userId;
    }
}

public class SameUserHandler : AuthorizationHandler<SameUserRequirement, int>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameUserRequirement requirement,
        int resource)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }
        
        var currentUserId = int.Parse(userIdClaim.Value);
        if (currentUserId == requirement.UserId)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
        
        return Task.CompletedTask;
    }
}
```

## Security Middleware

### Security Headers Middleware

```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;
    
    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
        {
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        }
        
        if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
        {
            context.Response.Headers.Add("X-Frame-Options", "DENY");
        }
        
        if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
        {
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        }
        
        if (!context.Response.Headers.ContainsKey("Strict-Transport-Security"))
        {
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }
        
        if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
        {
            context.Response.Headers.Add("Content-Security-Policy", 
                "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self'");
        }
        
        if (!context.Response.Headers.ContainsKey("Referrer-Policy"))
        {
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        }
        
        // Remove server information
        context.Response.Headers.Remove("Server");
        
        await _next(context);
    }
}

// Rate Limiting Middleware
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly IMemoryCache _cache;
    private readonly RateLimitOptions _options;
    
    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IMemoryCache cache,
        IOptions<RateLimitOptions> options)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
        _options = options.Value;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var cacheKey = $"rate_limit_{clientId}";
        
        var requestCount = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });
        
        if (requestCount >= _options.RequestLimit)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }
        
        _cache.Set(cacheKey, requestCount + 1);
        
        await _next(context);
    }
    
    private string GetClientId(HttpContext context)
    {
        // Try to get user ID from claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            return $"user_{userIdClaim.Value}";
        }
        
        // Fallback to IP address
        return $"ip_{context.Connection.RemoteIpAddress}";
    }
}

public class RateLimitOptions
{
    public int RequestLimit { get; set; } = 100;
    public int WindowSizeMinutes { get; set; } = 1;
}
```

## API Controllers with Security

### Authentication Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(ITokenService tokenService, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _tokenService.AuthenticateAsync(request);
            
            if (!result.Success)
            {
                return Unauthorized(new { message = result.Message });
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user {Username}", request.Username);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred during login" });
        }
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthenticationResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
            }
            
            var result = await _tokenService.RefreshTokenAsync(request.RefreshToken);
            
            if (!result.Success)
            {
                return Unauthorized(new { message = result.Message });
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred during token refresh" });
        }
    }
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        try
        {
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                await _tokenService.RevokeTokenAsync(request.RefreshToken);
            }
            
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred during logout" });
        }
    }
}
```

### Secure API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    
    [HttpGet]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving users" });
        }
    }
    
    [HttpGet("{id}")]
    [Authorize(Policy = "SameUser")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving the user" });
        }
    }
    
    [HttpPut("{id}")]
    [Authorize(Policy = "SameUser")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            return Ok(user);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while updating the user" });
        }
    }
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while deleting the user" });
        }
    }
}
```

## Configuration Files

### appsettings.json

```json
{
  "Jwt": {
    "Issuer": "YourApp",
    "Audience": "YourAppUsers",
    "Key": "ThisIsASecretKeyForJWTTokenGeneration123456789!@#$%^&*()",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "RateLimit": {
    "RequestLimit": 100,
    "WindowSizeMinutes": 1
  },
  "Security": {
    "RequireHttps": true,
    "EnableCORS": true,
    "AllowedOrigins": ["http://localhost:3000", "https://yourapp.com"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowedHeaders": ["*"]
  }
}
```

### appsettings.Development.json

```json
{
  "Jwt": {
    "Issuer": "YourApp-Dev",
    "Audience": "YourAppUsers-Dev",
    "Key": "DevelopmentSecretKeyForJWT123456789",
    "ExpirationMinutes": 1440
  },
  "Security": {
    "RequireHttps": false,
    "EnableCORS": true,
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5000", "http://localhost:5001"]
  }
}
```

## Security Best Practices

### Password Hashing

```csharp
public class PasswordService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, 12);
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

### Input Validation

```csharp
public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
    public string Password { get; set; }
}
```

### Anti-CSRF Protection

```csharp
// In Program.cs
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.HeaderName = "X-CSRF-TOKEN";
});

// In controller
[ValidateAntiForgeryToken]
[HttpPost]
public IActionResult SecureAction()
{
    return Ok();
}
```

## Testing Security

### Authentication Tests

```csharp
public class TokenServiceTests
{
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly TokenService _tokenService;
    
    public TokenServiceTests()
    {
        _jwtServiceMock = new Mock<IJwtService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        
        _tokenService = new TokenService(
            _jwtServiceMock.Object,
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            Mock.Of<ILogger<TokenService>>());
    }
    
    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "password123" };
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsActive = true,
            Role = "User"
        };
        
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);
        
        _jwtServiceMock.Setup(j => j.GenerateToken(user))
            .Returns("jwt-token");
        
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken())
            .Returns("refresh-token");
        
        // Act
        var result = await _tokenService.AuthenticateAsync(request);
        
        // Assert
        Assert.True(result.Success);
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("refresh-token", result.RefreshToken);
    }
    
    [Fact]
    public async Task AuthenticateAsync_InvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "wrongpassword" };
        
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync((User)null);
        
        // Act
        var result = await _tokenService.AuthenticateAsync(request);
        
        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid username or password", result.Message);
    }
}
```

### Integration Tests

```csharp
public class AuthenticationIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public AuthenticationIntegrationTests(TestWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginRequest = new
        {
            Username = "testuser",
            Password = "password123"
        };
        
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/auth/login", content);
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.RefreshToken);
    }
    
    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ProtectedEndpoint_WithToken_ReturnsSuccess()
    {
        // Arrange
        var loginResponse = await LoginAsync("testuser", "password123");
        var token = loginResponse.Token;
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/users");
        
        // Assert
        response.EnsureSuccessStatusCode();
    }
    
    private async Task<AuthenticationResult> LoginAsync(string username, string password)
    {
        var loginRequest = new { Username = username, Password = password };
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<AuthenticationResult>();
    }
}
```

## Best Practices

### Do's
- ✅ Use strong, unique JWT signing keys
- ✅ Implement proper token expiration and refresh
- ✅ Use HTTPS in production environments
- ✅ Validate all input data
- ✅ Implement proper error handling
- ✅ Use secure password hashing
- ✅ Implement rate limiting
- ✅ Add security headers
- ✅ Log security events
- ✅ Test security implementations

### Don'ts
- ❌ Store JWT secrets in client-side code
- ❌ Use weak encryption keys
- ❌ Skip token validation
- ❌ Ignore security headers
- ❌ Store passwords in plain text
- ❌ Skip input validation
- ❌ Ignore rate limiting
- ❌ Log sensitive information
- ❌ Skip security testing
- ❌ Use default configurations in production

## References

- [JWT Authentication in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt)
- [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [JWT Security Best Practices](https://auth0.com/blog/json-web-token-best-practices)

---

**This skill provides comprehensive guidance for implementing secure JWT authentication and authorization in .NET applications. Use it to create robust, secure, and maintainable authentication systems.**
