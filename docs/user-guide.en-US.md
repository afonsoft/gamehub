# Developer Guide — GameHub

## Introduction

This guide helps HTML5/WebGL game developers publish, manage, and monetize titles on the GameHub platform.

## Contents

1. [Getting started](#getting-started)
2. [Creating a game](#creating-a-game)
3. [Uploading builds](#uploading-builds)
4. [Integrating the SDK](#integrating-the-sdk)
5. [Publishing and moderation](#publishing-and-moderation)
6. [Analytics and earnings](#analytics-and-earnings)
7. [Multiplayer and chat](#multiplayer-and-chat)
8. [FAQ](#faq)

## Getting started

### Create an account

1. Visit the public GameHub portal.
2. Click **Register** and provide e-mail, name, and password.
3. Confirm your e-mail if required.
4. Log in to access the **Developer Portal**.

### Create a developer profile

In the Developer Portal, complete:

- Studio or developer name.
- Website and social links (optional).
- Tax/payout information for revenue sharing.

## Creating a game

1. In the Developer Portal, go to **Games > New Game**.
2. Fill in:
   - **Title**: public name of the game.
   - **Slug**: unique identifier used in the URL.
   - **Description**: short summary for the catalog.
   - **Category**: game genre.
   - **Age Rating**: content rating.
   - **Orientation**: portrait, landscape, or both.
3. Save as **Draft**.

### Required assets

- Icon (512x512 PNG).
- Cover/catalog image (1280x720 or 1920x1080).
- Screenshots (at least 3).
- Description in at least one supported language.

## Uploading builds

### Package requirements

- Format: ZIP file.
- Maximum size: defined by `MaxBuildPackageSizeBytes` in the instance.
- Required content: `index.html` at the root.
- Prohibited content: executables (`.exe`, `.dll`, `.bat`, `.cmd`, `.ps1`), server-side scripts, malware.

### Upload process

1. On the game page, open the **Builds** tab.
2. Drag or select the ZIP file.
3. Wait for automatic validation:
   - Check for `index.html`.
   - SHA-256 hash calculation.
   - Size and content checks.
4. The build will receive status **Pending**.

### Versions

- Each upload creates a new version.
- The most recent approved version can be set to **Live**.
- Rejected builds show the reason and allow a new upload.

## Integrating the SDK

### Initialization

```html
<script src="https://<gamehub-host>/assets/gameplay-bridge.js"></script>
<script>
  GameHubSDK.init({ gameId: 'your-game-id' });
</script>
```

### Required events

- `gameLoadingFinished`: when loading completes.
- `gameplayStart`: when the player starts a match.
- `gameplayStop`: when the match ends.
- `commercialBreakCompleted`: after a standard ad.
- `rewardedBreakCompleted`: after a rewarded ad.
- `gameErrorCaptured`: errors captured by the game.
- `measure`: custom analytics events.

### Example

```javascript
GameHubSDK.gameLoadingFinished();
GameHubSDK.gameplayStart();
// during gameplay
GameHubSDK.commercialBreakCompleted();
GameHubSDK.rewardedBreakCompleted({ success: true });
GameHubSDK.gameplayStop();
```

## Publishing and moderation

### Submit for review

1. Make sure there is an **Approved** build.
2. On the game page, click **Submit for Review**.
3. Add moderator notes if desired.
4. The game status changes to **Under Review**.

### Moderation statuses

| Status       | Meaning                                       |
|--------------|-----------------------------------------------|
| Draft        | Editable, not visible to the public.          |
| Under Review | Awaiting platform review.                     |
| Approved     | Approved but not yet published.               |
| Rejected     | Rejected; reason available in the panel.      |
| Published    | Visible in the catalog and playable.          |

### Update a published game

1. Upload a new build.
2. Once approved, click **Publish** to make it live.
3. The catalog and players automatically use the new version.

## Analytics and earnings

### Dashboard

Go to **Dashboard** to view:

- Total plays and unique players.
- Finished sessions and average duration.
- Gameplay events (start, loading, errors).
- Commercial and rewarded breaks.

### Filters

- Period (from/to).
- Country.
- Device type.
- Traffic source.
- UTM campaign.

### Earnings

The **Earnings** tab shows:

- Estimated gross revenue.
- Developer share.
- Platform share.
- Breakdown per game and per day.

Important: values are **estimated** and do not represent confirmed payouts.

### Export CSV

1. Apply the desired filters.
2. Click **Export CSV**.
3. The file will be downloaded with the period data.

## Multiplayer and chat

### Multiplayer

Use `GameHubSDK.multiplayer` to:

- Create or join rooms.
- Send game messages.
- Synchronize state across players.

### Chat

Use `GameHubSDK.chat` to:

- Send text messages in rooms.
- Report inappropriate messages.
- Receive automatic moderation for forbidden words.

## FAQ

**Do I need to host the game on GameHub?**
Yes. The build is hosted on GameHub infrastructure and executed inside a protected iframe.

**Is WebGL supported?**
Yes. HTML5/WebGL builds are supported as long as they run from an `index.html`.

**How does revenue sharing work?**
The split depends on the revenue contract configured for the game (non-exclusive or exclusive).

**How do I report a bug?**
Use the support form in the Developer Portal or e-mail the platform team.
