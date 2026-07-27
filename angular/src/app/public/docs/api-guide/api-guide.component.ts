import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-api-guide',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './api-guide.component.html',
})
export class ApiGuideComponent {
  readonly examples = {
    baseUrl: 'https://gamehub-api.afonsoft.dev/api',
    swagger: 'https://gamehub-api.afonsoft.dev/swagger',
    auth: `// 1. Authenticate
curl -X POST https://gamehub-api.afonsoft.dev/api/TokenAuth/Authenticate \\
  -H 'Content-Type: application/json' \\
  -d '{"userNameOrEmailAddress":"player@example.com","password":"*****"}'

// 2. Use the access token in subsequent requests
curl https://gamehub-api.afonsoft.dev/api/services/app/PlayerAccount/GetPlayerProfile \\
  -H 'Authorization: Bearer {accessToken}'`,
    catalog: `GET /api/services/app/GameCatalog/GetGames
GET /api/services/app/GameCatalog/GetBySlug?slug={slug}
GET /api/services/app/GameCatalog/Search?Query={q}&Device={device}&Orientation={orientation}&MinRating={rating}`,
    gameplay: `// Start a play session
POST /api/services/app/Gameplay/StartSession
{
  "gameId": "{gameId}",
  "deviceType": "Desktop",
  "browser": "Mozilla/5.0...",
  "referrer": "https://gamehub.afonsoft.dev"
}

// Send a gameplay event
POST /api/services/app/Gameplay/Event
{
  "sessionId": "{sessionId}",
  "eventType": 2,
  "payloadJson": "{}"
}

// Submit a score
POST /api/services/app/Leaderboard/SubmitScore
{
  "gameId": "{gameId}",
  "score": 1234,
  "metadataJson": "{\"level\": 5}"
}`,
    player: `GET  /api/services/app/PlayerAccount/GetFavorites
GET  /api/services/app/PlayerAccount/GetRecent?Max=20
POST /api/services/app/PlayerAccount/ToggleFavorite
{
  "gameId": "{gameId}"
}
POST /api/services/app/PlayerAccount/TrackPlay
{
  "gameId": "{gameId}"
}`,
    errors: `400 Bad Request - validation errors (see details array)
401 Unauthorized - missing or expired Bearer token
403 Forbidden - user has no permission for the requested tenant
404 Not Found - game, build or resource not found
500 Internal Server Error - report with UTC time and request path`,
  };
}
