# 04 - Modelagem de Dados

## Entidades principais

### Game (→ Catalog Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| Title | `string` | Required, max 256 | Título do jogo |
| Slug | `string` | Required, max 256, unique | Slug URL-friendly |
| ShortDescription | `string` | Required, max 500 | Descrição curta |
| Description | `string?` | Nullable, max 4000 | Descrição completa |
| Instructions | `string?` | Nullable, max 2000 | Instruções de como jogar |
| Status | `GameStatus` | Required, default Draft | Status do jogo |
| AgeRating | `string` | Required, max 32 | Classificação indicativa |
| Orientation | `GameOrientation` | Required, default Both | Orientação suportada |
| SupportsDesktop | `bool` | Required, default true | Suporta desktop |
| SupportsMobile | `bool` | Required, default false | Suporta mobile |
| SupportsTablet | `bool` | Required, default false | Suporta tablet |
| ThumbnailUrl | `string?` | Nullable, max 512 | URL da miniatura (600x400) |
| HeroImageUrl | `string?` | Nullable, max 512 | URL da imagem hero (1920x1080) |
| DeveloperProfileId | `Guid` | FK → DeveloperProfile.Id, required | Desenvolvedor dono |
| PublishedBuildId | `Guid?` | FK → GameBuild.Id, nullable | Build publicado atual |
| TotalPlays | `long` | Required, default 0 | Contador de plays |
| AverageRating | `double?` | Nullable | Média de avaliações |
| CreationTime | `DateTime` | Required | Data de criação |
| LastModificationTime | `DateTime?` | Nullable | Última modificação |
| IsDeleted | `bool` | Required, default false | Soft delete |

**Índices:**
- `IX_Game_Slug` → Slug (unique)
- `IX_Game_Status_CreationTime` → Status + CreationTime (para ordenação)
- `IX_Game_DeveloperProfileId` → DeveloperProfileId
- `IX_Game_PublishedBuildId` → PublishedBuildId

**Relacionamentos:**
- `DeveloperProfileId` → `DeveloperProfile.Id` (required, cascade: restrict)
- `PublishedBuildId` → `GameBuild.Id` (nullable, set null on delete)
- 1:N → `GameBuild` (via GameBuild.GameId)
- M:N → `Category` (via GameCategory)
- M:N → `Tag` (via GameTag)
- 1:N → `PlaySession`
- 1:N → `GameMetricSnapshot`
- 1:N → `GamePlacement`

---

### GameBuild (→ Build Management Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| GameId | `Guid` | FK → Game.Id, required, cascade delete | Jogo pai |
| Version | `string` | Required, max 32 | Semver (ex: 1.0.0) |
| BuildNumber | `int` | Required, auto-increment | Número sequencial do build |
| Status | `GameBuildStatus` | Required, default Uploaded | Status do build |
| OriginalPackageUrl | `string` | Required, max 1024 | URL do pacote original (privado) |
| PublicBaseUrl | `string?` | Nullable, max 1024 | URL pública dos arquivos extraídos |
| IndexHtmlPath | `string?` | Nullable, max 512 | Caminho do index.html no pacote |
| SizeBytes | `long` | Required | Tamanho em bytes |
| HashSha256 | `string` | Required, max 128 | Hash SHA-256 do pacote |
| ValidationSummary | `string?` | Nullable, max 4000 | JSON com resultado da validação |
| CreationTime | `DateTime` | Required | Data do upload |
| PublishedTime | `DateTime?` | Nullable | Data da publicação |
| IsDeleted | `bool` | Required, default false | Soft delete |

**Índices:**
- `IX_GameBuild_GameId_Version` → GameId + Version (unique)
- `IX_GameBuild_GameId_Status` → GameId + Status

**Relacionamentos:**
- `GameId` → `Game.Id` (required, cascade delete)

---

### Category (→ Catalog Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| Name | `string` | Required, max 128 | Nome da categoria |
| Slug | `string` | Required, max 128, unique | Slug URL-friendly |
| SortOrder | `int` | Required, default 0 | Ordem de exibição |
| IsActive | `bool` | Required, default true | Se está ativa |
| IsDeleted | `bool` | Required, default false | Soft delete |

**Índices:**
- `IX_Category_Slug` → Slug (unique)
- `IX_Category_SortOrder` → SortOrder (para ordenação)

**Relacionamentos:**
- M:N → `Game` (via GameCategory)

---

### Tag (→ Catalog Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| Name | `string` | Required, max 64 | Nome da tag |
| Slug | `string` | Required, max 64, unique | Slug URL-friendly |
| IsDeleted | `bool` | Required, default false | Soft delete |

**Índices:**
- `IX_Tag_Slug` → Slug (unique)

**Relacionamentos:**
- M:N → `Game` (via GameTag)

---

### GameCategory (→ Catalog Context)

**Tabela de junção** para M:N entre Game e Category.

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| GameId | `Guid` | FK → Game.Id, PK composto, cascade delete | Jogo |
| CategoryId | `Guid` | FK → Category.Id, PK composto, cascade delete | Categoria |

**PK composta:** `(GameId, CategoryId)`

---

### GameTag (→ Catalog Context)

**Tabela de junção** para M:N entre Game e Tag.

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| GameId | `Guid` | FK → Game.Id, PK composto, cascade delete | Jogo |
| TagId | `Guid` | FK → Tag.Id, PK composto, cascade delete | Tag |

**PK composta:** `(GameId, TagId)`

---

### GamePlacement (→ Catalog Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| GameId | `Guid` | FK → Game.Id, required | Jogo |
| PlacementType | `GamePlacementType` | Required | Tipo de colocação |
| SortOrder | `int` | Required, default 0 | Ordem na seção |
| IsActive | `bool` | Required, default true | Se está ativo |
| StartDate | `DateTime?` | Nullable | Data de início da promoção |
| EndDate | `DateTime?` | Nullable | Data fim da promoção |

**Relacionamentos:**
- `GameId` → `Game.Id` (required, cascade delete)

---

### DeveloperProfile (→ Developer Portal Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| UserId | `Guid` | FK → AbpUsers.Id, required, unique | Usuário dono do perfil |
| DisplayName | `string` | Required, max 128 | Nome público |
| LegalName | `string?` | Nullable, max 256 | Nome legal (PJ) |
| WebsiteUrl | `string?` | Nullable, max 512 | Site do desenvolvedor |
| SupportEmail | `string?` | Nullable, max 256 | Email de suporte |
| Status | `DeveloperProfileStatus` | Required, default Pending | Status do perfil |
| IsDeleted | `bool` | Required, default false | Soft delete |

**Índices:**
- `IX_DeveloperProfile_UserId` → UserId (unique)

**Unique constraints:**
- `UX_DeveloperProfile_UserId` → UserId

**Relacionamentos:**
- `UserId` → `AbpUsers.Id` (required, cascade: restrict)
- 1:N → `Game` (via Game.DeveloperProfileId)

---

### PlaySession (→ Gameplay Analytics Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| GameId | `Guid` | FK → Game.Id, required | Jogo jogado |
| UserId | `Guid?` | Nullable | Usuário autenticado |
| AnonymousIdHash | `string?` | Nullable, max 128 | SHA256 do anonymous ID |
| StartedAt | `DateTime` | Required | Início da sessão |
| EndedAt | `DateTime?` | Nullable | Fim da sessão |
| DeviceType | `string` | Required, max 32 | "desktop", "mobile", "tablet" |
| Browser | `string` | Required, max 64 | Nome do browser |
| CountryCode | `string?` | Nullable, max 2 | ISO 3166-1 alpha-2 |
| Referrer | `string?` | Nullable, max 1024 | URL de referrer |

**Relacionamentos:**
- `GameId` → `Game.Id` (required, restrict)
- 1:N → `GameplayEvent`

**Retention:** Hard delete após 12 meses.

---

### GameplayEvent (→ Gameplay Analytics Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| PlaySessionId | `Guid` | FK → PlaySession.Id, required | Sessão pai |
| GameId | `Guid` | FK → Game.Id, required | Jogo |
| EventType | `GameplayEventType` | Required | Tipo do evento |
| EventName | `string?` | Nullable, max 128 | Nome customizado do evento |
| PayloadJson | `string?` | Nullable, max 4000 | JSON com dados extras |
| OccurredAt | `DateTime` | Required | Quando ocorreu |

**Relacionamentos:**
- `PlaySessionId` → `PlaySession.Id` (required, cascade delete)
- `GameId` → `Game.Id` (required, restrict)

**Retention:** Hard delete após 6 meses.

---

### GameMetricSnapshot (→ Gameplay Analytics Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| GameId | `Guid` | FK → Game.Id, required | Jogo |
| Date | `DateTime` | Required | Data do snapshot (gr diária) |
| Plays | `long` | Required, default 0 | Total de plays no dia |
| UniquePlayers | `long` | Required, default 0 | Jogadores únicos no dia |
| AvgDurationSeconds | `double` | Required, default 0 | Duração média em segundos |
| LoadingFinishedCount | `long` | Required, default 0 | Eventos gameLoadingFinished |
| ErrorCount | `long` | Required, default 0 | Eventos gameErrorCaptured |
| CommercialBreakCount | `long` | Required, default 0 | Ads comerciais exibidos |
| RewardedBreakCount | `long` | Required, default 0 | Ads reward exibidos |

**Índices:**
- `IX_GameMetricSnapshot_GameId_Date` → GameId + Date (para queries de série temporal)

**Granularidade:** Diária. Cada linha representa um dia para um jogo. Agregação de eventos brutos.

**Retention:** 24 meses.

---

### LeaderboardEntry (→ Gameplay Analytics Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| GameId | `Guid` | FK → Game.Id, required | Jogo |
| UserId | `Guid` | FK → AbpUsers.Id, required | Usuário |
| Score | `long` | Required | Melhor score |
| MetadataJson | `string?` | Nullable, max 4000 | Dados extras do score |
| CreatedAt | `DateTime` | Required | Primeiro score |
| UpdatedAt | `DateTime` | Required | Última atualização |

**Índices:**
- `IX_LeaderboardEntry_GameId_Score` → GameId + Score DESC (para ranking)
- `UX_LeaderboardEntry_GameId_UserId` → GameId + UserId (unique)

**Unique constraints:**
- `UX_LeaderboardEntry_GameId_UserId` → (GameId, UserId) — um score por usuário por jogo

**Relacionamentos:**
- `GameId` → `Game.Id` (required, cascade delete)
- `UserId` → `AbpUsers.Id` (required, cascade: restrict)

**Redis:** Sorted Set mantém ranking online (`leaderboard:{gameId}`, score = Score, member = UserId). Snapshots periódicos para o banco.

---

### ModerationReview (→ Moderation Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| GameId | `Guid` | FK → Game.Id, required | Jogo em revisão |
| GameBuildId | `Guid` | FK → GameBuild.Id, required | Build em revisão |
| ReviewerUserId | `Guid?` | Nullable | Revisor (null = pendente) |
| Status | `ModerationReviewStatus` | Required, default Pending | Status da revisão |
| Decision | `ModerationDecision?` | Nullable | Decisão final |
| Notes | `string?` | Nullable, max 2000 | Notas do revisor |
| CreatedAt | `DateTime` | Required | Criação da revisão |
| CompletedAt | `DateTime?` | Nullable | Conclusão da revisão |
| IsDeleted | `bool` | Required, default false | Soft delete |

**Relacionamentos:**
- `GameId` → `Game.Id` (required, restrict)
- `GameBuildId` → `GameBuild.Id` (required, restrict)
- `ReviewerUserId` → `AbpUsers.Id` (nullable, set null on delete)
- 1:N → `UserReport` (via UserReport.ModerationReviewId)

---

### UserReport (→ Moderation Context)

| Campo | Tipo C# | Constraints | Descrição |
|-------|---------|------------|-----------|
| Id | `Guid` | PK | Identificador único |
| GameId | `Guid` | FK → Game.Id, required | Jogo reportado |
| UserId | `Guid?` | Nullable | Usuário que reportou (null = anônimo) |
| ModerationReviewId | `Guid?` | FK → ModerationReview.Id, nullable | Revisão associada |
| Reason | `string` | Required, max 128 | Motivo do report |
| Description | `string?` | Nullable, max 2000 | Descrição detalhada |
| Status | `UserReportStatus` | Required, default Open | Status do report |
| CreatedAt | `DateTime` | Required | Data do report |
| ResolvedAt | `DateTime?` | Nullable | Data da resolução |
| IsDeleted | `bool` | Required, default false | Soft delete |

**Relacionamentos:**
- `GameId` → `Game.Id` (required, restrict)
- `UserId` → `AbpUsers.Id` (nullable, set null on delete)
- `ModerationReviewId` → `ModerationReview.Id` (nullable, set null on delete)

**Retention:** 5 anos (compliance de moderação).

---

## Enums

```csharp
public enum GameStatus
{
    Draft = 0,
    InReview = 1,
    Published = 2,
    Rejected = 3,
    Suspended = 4,
    Archived = 5
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
    GameMeasuredEvent = 9
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
```

## Value Objects

### Slug

```csharp
public class Slug : ValueObject
{
    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Slug Create(string input)
    {
        var normalized = input
            .ToLowerInvariant()
            .Trim()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove caracteres especiais, mantém apenas [a-z0-9-]
        normalized = Regex.Replace(normalized, @"[^a-z0-9\-]", "");

        // Remove múltiplos hífens
        normalized = Regex.Replace(normalized, @"-{2,}", "-");

        // Remove hífens no início e fim
        normalized = normalized.Trim('-');

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Slug cannot be empty.");

        if (normalized.Length > 256)
            throw new ArgumentException("Slug cannot exceed 256 characters.");

        return new Slug(normalized);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### AgeRating

```csharp
public class AgeRating : ValueObject
{
    public string Value { get; }

    private static readonly string[] ValidRatings = { "Everyone", "Teen", "Mature" };

    private AgeRating(string value) => Value = value;

    public static AgeRating Create(string rating)
    {
        if (!ValidRatings.Contains(rating, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Invalid age rating: {rating}. Valid: {string.Join(", ", ValidRatings)}");

        return new AgeRating(ValidRatings.First(r =>
            r.Equals(rating, StringComparison.OrdinalIgnoreCase)));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### BuildVersion

```csharp
public class BuildVersion : ValueObject
{
    public string Value { get; }

    private BuildVersion(string value) => Value = value;

    public static BuildVersion Create(string version)
    {
        if (!Regex.IsMatch(version, @"^\d+\.\d+\.\d+$"))
            throw new ArgumentException($"Invalid semver: {version}. Expected format: MAJOR.MINOR.PATCH");

        return new BuildVersion(version);
    }

    public BuildVersion IncrementMajor() =>
        ParseParts((major, minor, patch) => new BuildVersion($"{major + 1}.0.0"));

    public BuildVersion IncrementMinor() =>
        ParseParts((major, minor, patch) => new BuildVersion($"{major}.{minor + 1}.0"));

    public BuildVersion IncrementPatch() =>
        ParseParts((major, minor, patch) => new BuildVersion($"{major}.{minor}.{patch + 1}"));

    private T ParseParts<T>(Func<int, int, int, T> func)
    {
        var parts = Value.Split('.').Select(int.Parse).ToArray();
        return func(parts[0], parts[1], parts[2]);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### GameOrientation (já é enum)

```csharp
public enum GameOrientation
{
    Landscape = 0,
    Portrait = 1,
    Both = 2
}
```

## Unique Constraints

| Entidade | Constraint | Campo(s) |
|----------|-----------|----------|
| Game | `UX_Game_Slug` | Slug |
| GameBuild | `UX_GameBuild_GameId_Version` | GameId + Version |
| Category | `UX_Category_Slug` | Slug |
| Tag | `UX_Tag_Slug` | Slug |
| DeveloperProfile | `UX_DeveloperProfile_UserId` | UserId |
| LeaderboardEntry | `UX_LeaderboardEntry_GameId_UserId` | GameId + UserId |

## Estratégia EF Core

- Usar configurações por entidade com `IEntityTypeConfiguration<T>`.
- Evitar Fluent API gigante dentro do DbContext.
- Usar migrations versionadas.
- Criar seed inicial para categorias padrão e permissões ABP.
- Soft delete para entidades administrativas e de domínio.
- Hard delete para dados de telemetria (PlaySession, GameplayEvent).
- Value objects configurados via OwnsOne ou conversão de valor.

### Configuração de Value Objects no EF Core

```csharp
// GameConfiguration.cs
builder.Property(g => g.Slug)
    .HasConversion(
        v => v.Value,
        v => Slug.Create(v))
    .HasMaxLength(256)
    .IsRequired();
```

### Configuração de Tabelas de Junção

```csharp
// GameCategoryConfiguration.cs
builder.HasKey(gc => new { gc.GameId, gc.CategoryId });

builder.HasOne(gc => gc.Game)
    .WithMany(g => g.GameCategories)
    .HasForeignKey(gc => gc.GameId)
    .OnDelete(DeleteBehavior.Cascade);

builder.HasOne(gc => gc.Category)
    .WithMany()
    .HasForeignKey(gc => gc.CategoryId)
    .OnDelete(DeleteBehavior.Cascade);
```

### Configuração de Unique Constraints

```csharp
// DeveloperProfileConfiguration.cs
builder.HasIndex(dp => dp.UserId)
    .IsUnique();

// LeaderboardEntryConfiguration.cs
builder.HasIndex(le => new { le.GameId, le.UserId })
    .IsUnique();
```

### Seed Data

```csharp
// Categorias padrão
new Category { Name = "Action", Slug = "action", SortOrder = 1, IsActive = true };
new Category { Name = "Puzzle", Slug = "puzzle", SortOrder = 2, IsActive = true };
new Category { Name = "Racing", Slug = "racing", SortOrder = 3, IsActive = true };
new Category { Name = "Strategy", Slug = "strategy", SortOrder = 4, IsActive = true };
new Category { Name = "Adventure", Slug = "adventure", SortOrder = 5, IsActive = true };
new Category { Name = "Sports", Slug = "sports", SortOrder = 6, IsActive = true };
new Category { Name = "Board", Slug = "board", SortOrder = 7, IsActive = true };
```

### Retenção de Dados (Implementation)

```csharp
// Background job para cleanup de dados antigos
public class DataRetentionCleanupJob : IBackgroundJob
{
    public void Execute(JobArgs args)
    {
        // PlaySession: hard delete após 12 meses
        // GameplayEvent: hard delete após 6 meses
        // GameMetricSnapshot: manter 24 meses
        // RefreshToken: auto-expira via TTL Redis
        // BlacklistedToken: auto-expira via TTL Redis
    }
}
```
