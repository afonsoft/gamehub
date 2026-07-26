# Developer Publishing Workflow Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Connect the developer build history to version details, validation, Preview, Inspector, review feedback, and safe submission without duplicating existing backend services.

**Architecture:** Reuse `DeveloperGameAppService`, `GamePreviewAppService`, `InspectorAppService`, and the existing `GameBuild`/`ModerationReview` aggregates. Add only the missing review-history projection and frontend orchestration; keep preview tokens server-generated and never persist them in Angular state. The developer builds screen remains the single workflow entry point.

**Tech Stack:** ASP.NET Boilerplate, EF Core, C# 14, xUnit/Shouldly, Angular 20, TypeScript, RxJS.

---

### Task 1: Add the review-history contract

**Files:**
- Create: `Api/src/GameHub.Application/Developer/Dto/DeveloperReviewHistoryItemDto.cs`
- Modify: `Api/src/GameHub.Application/Developer/IDeveloperGameAppService.cs`
- Modify: `Api/src/GameHub.Application/Developer/DeveloperGameAppService.cs`
- Test: `Api/test/GameHub.Tests/GameHub/Application/DeveloperGameAppService_Tests.cs`

- [ ] **Step 1: Write the failing integration test**

Add a test that seeds two `ModerationReview` records for the current developer's game and asserts that `GetReviewHistoryAsync(gameId)` returns both records ordered newest first, including build id, status, notes, and timestamps.

- [ ] **Step 2: Run the focused test and verify it fails**

Run:

```bash
dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj --filter FullyQualifiedName~DeveloperGameAppService_Tests
```

Expected: compilation failure because `GetReviewHistoryAsync` and its DTO do not exist.

- [ ] **Step 3: Add the DTO and interface method**

Define a DTO with XML documentation and these properties:

```csharp
public Guid Id { get; set; }
public Guid GameId { get; set; }
public Guid? GameBuildId { get; set; }
public string Status { get; set; }
public string Notes { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime? CompletedAt { get; set; }
```

Add `Task<List<DeveloperReviewHistoryItemDto>> GetReviewHistoryAsync(Guid gameId);` to `IDeveloperGameAppService`.

- [ ] **Step 4: Implement authorized projection**

In `DeveloperGameAppService`, load the game, call `EnsureCurrentUserOwnsGameAsync`, query `_moderationReviewRepository` by `GameId` and non-deleted state, order by `CreationTime` descending, and map the result into the DTO. Do not expose internal moderation fields.

- [ ] **Step 5: Run the focused test and verify it passes**

Run the same focused command. Expected: all selected tests pass.

- [ ] **Step 6: Commit**

```bash
git add Api/src/GameHub.Application/Developer/Dto/DeveloperReviewHistoryItemDto.cs Api/src/GameHub.Application/Developer/IDeveloperGameAppService.cs Api/src/GameHub.Application/Developer/DeveloperGameAppService.cs Api/test/GameHub.Tests/GameHub/Application/DeveloperGameAppService_Tests.cs
git commit -m "feat(developer): expose review history"
```

### Task 2: Expose version and workflow data to Angular

**Files:**
- Modify: `angular/src/app/core/services/developer.service.ts`
- Modify: `angular/src/app/developer/builds/builds.component.ts`
- Modify: `angular/src/app/developer/builds/builds.component.html`
- Modify: `angular/src/app/developer/builds/builds.component.css`
- Test: `angular/src/app/core/services/developer.service.spec.ts`

- [ ] **Step 1: Write failing service tests**

Add HTTP tests for:

```text
GET /api/services/app/DeveloperGame/GetVersions?gameId={id}
GET /api/services/app/DeveloperGame/GetReviewHistory?gameId={id}
POST /api/services/app/DeveloperGame/CreatePreviewTokenForBuild
POST /api/services/app/DeveloperGame/StartInspectorSessionForBuild
```

Assert that ABP `{ result: ... }` envelopes are unwrapped and that preview results are not cached.

- [ ] **Step 2: Run the focused Angular test and verify it fails**

Run:

```bash
cd angular
npx ng test --watch=false --browsers=ChromeHeadlessNoSandbox --include='src/app/core/services/developer.service.spec.ts'
```

Expected: test discovery or compilation failure because the spec file/method under test is absent.

- [ ] **Step 3: Add typed review-history models and service method**

Add `DeveloperReviewHistoryItem` and `getReviewHistory(gameId)` to `DeveloperService`, using the existing `unwrap` helper and the existing endpoint naming convention.

- [ ] **Step 4: Add explicit action state**

Track one action at a time per build using a `Set<string>` or equivalent typed state. Preview and Inspector actions must set and clear loading state in both success and error paths. They must use `window.open` only after the server returns a token/session.

- [ ] **Step 5: Render the workflow**

Update the builds screen to:

- show the published build badge;
- show validation summary fields and warnings when available;
- render Preview and Inspector actions only for builds with the required identifiers;
- render review history below the build table;
- show retryable load errors without clearing existing data;
- replace `alert()`/`prompt()` with the existing reusable notification/confirmation pattern if present; otherwise create the smallest shared developer notification primitive.

- [ ] **Step 6: Run focused Angular tests and build**

Run:

```bash
cd angular
npx ng test --watch=false --browsers=ChromeHeadlessNoSandbox --include='src/app/core/services/developer.service.spec.ts'
npm run build
```

Expected: selected tests pass and production build exits with code 0.

- [ ] **Step 7: Commit**

```bash
git add angular/src/app/core/services/developer.service.ts angular/src/app/developer/builds angular/src/app/core/services/developer.service.spec.ts
git commit -m "feat(developer): connect version workflow actions"
```

### Task 3: Document and verify the workflow

**Files:**
- Modify: `angular/src/app/public/docs/user-guide/user-guide.component.html`
- Modify: `angular/public/i18n/pt-BR.json`
- Modify: `angular/public/i18n/en-US.json`
- Modify: `docs/agent-execution-log.md`

- [ ] **Step 1: Add workflow documentation**

Document the exact sequence: upload, validation, Preview, Inspector, approve build, submit for review, review feedback, and publish. State that Preview does not publish a game and that Earnings are estimates.

- [ ] **Step 2: Verify translations and links**

Run:

```bash
rg -n "docs\\.ug\\.(preview|inspector|review|publish)" angular/src/app/public/docs/user-guide angular/public/i18n
git diff --check
```

Expected: every new key exists in both locales and no whitespace errors are reported.

- [ ] **Step 3: Run backend verification**

```bash
dotnet build Api/GameHub.sln
dotnet test Api/GameHub.sln --no-build
```

Expected: build succeeds and the full test suite reports zero failures.

- [ ] **Step 4: Update the execution log**

Record changed files, endpoint reuse, security decisions, and any environment limitation without claiming tests that were not executed.

- [ ] **Step 5: Review the plan against Spec 35**

Confirm that versions, Preview, Inspector, validation visibility, review history, authorization, token handling, and User Guide coverage are all implemented or explicitly listed as remaining work.
