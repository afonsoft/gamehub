---
name: testing-gamehub
description: How to end-to-end test the GameHub admin UI and SignalR backend locally
---

# End-to-end testing GameHub admin + SignalR

## Devin Secrets Needed

None beyond the standard checkout. The local stack uses seeded credentials:

- Host admin login: `admin` / `123qwe`
- First login forces a password reset; set any compliant password, e.g. `NewPass123!`

## One-time local stack

1. Start infrastructure:
   ```bash
   docker compose -f docker-compose.infra.yml up -d
   ```
2. Run the .NET backend. Use `dotnet run` from the host project (not the built DLL) so `Properties/launchSettings.json` is honored:
   ```bash
   cd Api/src/GameHub.Web.Host
   dotnet run -c Release
   ```
3. Run the admin frontend with Node 20:
   ```bash
   cd angular-admin/GameHub.UI
   nvm use 20
   npm install --legacy-peer-deps
   npm run start
   ```
   `ng serve` binds to `localhost` and, in some environments, resolves to the IPv6 loopback (`[::1]:8001`) only.

## Known workarounds

- **`ng serve` source-map BOM error.** If `devtools-ignore-plugin.js` throws `Unexpected token '﻿' ... is not valid JSON`, strip the UTF-8 BOM from:
  - `node_modules/@angular-devkit/build-angular/src/tools/webpack/plugins/devtools-ignore-plugin.js`
  - `.map` files under `src/assets` that contain a BOM.

- **IPv6-only `ng serve` binding.** If Chrome cannot reach `127.0.0.1:8000` (`ERR_CONNECTION_REFUSED`), navigate to `http://localhost:8000`. When editing `appconfig.Local.json`, set `appBaseUrl` to the same host that `ng serve` is actually listening on (use `ss -ltnp | grep :8000` to confirm). Otherwise static image URLs may fail.

- **Admin `m-switch` toggle duplication.** Verify with the dedicated harness `dist/prod/mswitch-test.html` (served in the repo) or by inspecting `.m-switch-label` `:before`/`:after` content.

- **`commonlookupmodal` auto-opens after login.** It blocks the UI and its close buttons may not respond. This is preexisting and not part of the PR under test; suppress it with an injected `display:none !important` CSS rule only when necessary for testing.

## What to verify with DevTools

- **SignalR:** filter Network by `signalr-chat`. Expect:
  - `POST .../signalr-chat/negotiate?negotiateVersion=1` → `200 OK`
  - `signalr-chat?id=...` WebSocket → `101`
  - Console: `Chat reconnected. ConnectionId: ...`

- **Fonts:** filter Console for `font` or `decode`. There should be no `Failed to decode downloaded font` messages.

- **aria-hidden:** filter Console for `aria-hidden`. There should be no `Blocked aria-hidden on an element` warnings. Also inspect modal root `div` elements for the absence of `aria-hidden="true"`.

- **m-switch:** one toggle control per labeled item; no duplicate pseudo-element knob.
