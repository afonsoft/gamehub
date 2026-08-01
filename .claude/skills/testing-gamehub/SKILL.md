---
name: testing-gamehub
description: How to end-to-end test the GameHub admin UI and public UI locally, including EAF 9.4.3 tenant-registration gotchas
---

# End-to-end testing GameHub admin + public UI

## Devin Secrets Needed

None beyond the standard checkout. Local stack uses seeded credentials:

- Host admin login: `admin` / `123qwe`
- First login forces a password reset; set any compliant password, e.g. `NewPass123!`

## One-time local stack

1. Start infrastructure:
   ```bash
   docker compose -f docker-compose.infra.yml up -d
   ```
2. Build and migrate:
   ```bash
   cd Api
   dotnet build GameHub.sln -c Release
   EafMigrator=LOCAL ConnectionStrings__LOCAL="Host=localhost;Port=5432;Database=gamehub;Username=gamehub;Password=change-me" dotnet src/GameHub.Migrator/bin/Release/net10.0/GameHub.Migrator.dll
   ```
3. Run the .NET backend. Use `dotnet run` from the host project so `Properties/launchSettings.json` is honored:
   ```bash
   cd Api/src/GameHub.Web.Host
   ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://+:8001 dotnet run -c Release --no-build
   ```
4. Start the public UI. `angular.json` already wires `proxy.conf.json` so `/api` is proxied to the backend:
   ```bash
   cd angular
   npm start -- --host 0.0.0.0 --port 4200
   ```
5. Run the admin UI with Node 22:
   ```bash
   cd angular-admin/GameHub.UI
   npm install --legacy-peer-deps
   npm run start
   ```
   `ng serve` binds to `localhost:8000`. Edit `src/assets/appconfig.Local.json` so `remoteServiceBaseUrl` points to `http://localhost:8001` and `appBaseUrl` to `http://localhost:8000`.

## Known workarounds

- **`ng serve` source-map BOM error.** If `devtools-ignore-plugin.js` throws `Unexpected token '﻿' ... is not valid JSON`, strip the UTF-8 BOM from:
  - `node_modules/@angular-devkit/build-angular/src/tools/webpack/plugins/devtools-ignore-plugin.js`
  - `.map` files under `src/assets` that contain a BOM.

- **IPv6-only `ng serve` binding.** If Chrome cannot reach `127.0.0.1:8000` (`ERR_CONNECTION_REFUSED`), navigate to `http://localhost:8000`. When editing `appconfig.Local.json`, set `appBaseUrl` to the same host that `ng serve` is actually listening on (use `ss -ltnp | grep :8000` to confirm).

- **Admin `m-switch` toggle duplication.** Verify with the dedicated harness `dist/prod/mswitch-test.html` (served in the repo) or by inspecting `.m-switch-label` `:before`/`:after` content.

- **`commonlookupmodal` auto-opens after login.** It blocks the UI and its close buttons may not respond. Suppress it with an injected `display:none !important` CSS rule only when necessary for testing.

## EAF 9.4.3 tenant-registration testing notes

- **Swagger now loads correctly** after `CustomSchemaIds(type => type.FullName)` was added; `TenantJoinRequest` and `Registration` controllers are reachable.

- **`/api/services/app/TenantJoinRequest/*` endpoints are no longer ambiguous** because GameHub's obsolete `TenantJoinRequestAppService` was removed and the app now uses `Eaf.Middleware.MultiTenancy.TenantJoinRequest`.

- **Public UI player-default registration works** after `AuthService.register` was updated to call `HubAuthService.selectTenant` / `getAvailableTenants` instead of legacy `TokenAuth/Authenticate`.

- **Admin UI bootstrap now works** after `GameHubDbContext` was migrated to the EAF `TenantJoinRequest` and `UserTenantMembership` entities.

- **Public UI `JoinExisting` company `<select>` works** as long as the Angular dev-server is started with the provided `proxy.conf.json` so `GetAvailableTenants` reaches the backend.

- **Admin UI has no TenantJoinRequest management page yet.** Pending requests can be listed/approved through the generated `TenantJoinRequestServiceProxy` or the Swagger UI (`POST /api/services/app/TenantJoinRequest/Approve`).

## What to verify with DevTools

- **SignalR:** filter Network by `signalr-chat`. Expect:
  - `POST .../signalr-chat/negotiate?negotiateVersion=1` → `200 OK`
  - `signalr-chat?id=...` WebSocket → `101`
  - Console: `Chat reconnected. ConnectionId: ...`

- **Fonts:** filter Console for `font` or `decode`. There should be no `Failed to decode downloaded font` messages.

- **aria-hidden:** filter Console for `aria-hidden`. There should be no `Blocked aria-hidden on an element` warnings. Also inspect modal root `div` elements for the absence of `aria-hidden="true"`.

- **m-switch:** one toggle control per labeled item; no duplicate pseudo-element knob.
