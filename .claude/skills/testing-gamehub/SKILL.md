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
4. Proxy the public frontend to the backend. Create `angular/proxy.conf.json`:
   ```json
   {
     "/api": {
       "target": "http://localhost:8001",
       "secure": false,
       "changeOrigin": true,
       "logLevel": "silent"
     }
   }
   ```
   Then start the public UI:
   ```bash
   cd angular
   npx ng serve --port 8002 --proxy-config proxy.conf.json --host 0.0.0.0
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

- **Swagger definition may fail to generate** when `RegisterInput` exists in both `GameHub.Authorization.Dto` and `Eaf.Middleware.Authorization.Accounts.Dto`. API calls still work; Swagger UI may show `Fetch error`.

- **`/api/services/app/TenantJoinRequest/GetAvailableTenants` may return HTTP 500** with an `AmbiguousMatchException` because both EAF and GameHub expose a `TenantJoinRequestAppService`. This blocks the public UI join-request dropdown and the developer profile Companies/Tenants list.

- **Public UI player-default registration may fail after the API call succeeds.** `AuthService.register` falls back to legacy `/api/TokenAuth/Authenticate`, which cannot authenticate tenant-only users (players). Use the API directly or the `/api/hub/auth/*` flow to test login separately.

- **Admin UI bootstrap may fail with a DI exception** for `Eaf.Middleware.Authorization.Accounts.AccountAppService` because it depends on EAF's `TenantJoinRequest` and `UserTenantMembership` repositories, which are not registered in `GameHubDbContext`.

## What to verify with DevTools

- **SignalR:** filter Network by `signalr-chat`. Expect:
  - `POST .../signalr-chat/negotiate?negotiateVersion=1` → `200 OK`
  - `signalr-chat?id=...` WebSocket → `101`
  - Console: `Chat reconnected. ConnectionId: ...`

- **Fonts:** filter Console for `font` or `decode`. There should be no `Failed to decode downloaded font` messages.

- **aria-hidden:** filter Console for `aria-hidden`. There should be no `Blocked aria-hidden on an element` warnings. Also inspect modal root `div` elements for the absence of `aria-hidden="true"`.

- **m-switch:** one toggle control per labeled item; no duplicate pseudo-element knob.
