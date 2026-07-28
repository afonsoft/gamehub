import { Component } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppLocalizationService } from '@app/shared/common/localization/app-localization.service';

type Lang = 'pt' | 'en';

@Component({
  standalone: false,
  selector: 'app-gamehub-docs',
  templateUrl: './docs.component.html',
  animations: [appModuleAnimation()],
})
export class DocsComponent {
  lang: Lang = 'pt';

  readonly apiExamples = {
    auth: `POST /api/TokenAuth/Authenticate
{
  "userNameOrEmailAddress": "admin",
  "password": "***"
}`,
    catalog: `GET /api/services/app/GameCatalog/GetGames
GET /api/services/app/GameCatalog/GetBySlug?slug={slug}
GET /api/services/app/GameCatalog/Search?Query={q}`,
    gameplay: `POST /api/services/app/Gameplay/StartSession
{
  "gameId": "{gameId}",
  "deviceType": "Desktop"
}`,
  };

  readonly sdkExamples = {
    init: `GameHubSDK.init();
GameHubSDK.gameLoadingStarted();
GameHubSDK.gameLoadingFinished();
GameHubSDK.gameplayStart();`,
    ads: `await GameHubSDK.commercialBreakRequested();
const rewarded = await GameHubSDK.rewardedBreakRequested();`,
  };

  constructor(private readonly _localization: AppLocalizationService) {}

  setLang(lang: Lang): void {
    this.lang = lang;
  }

  l(key: string, ...args: any[]): string {
    return this._localization.l(key, ...args);
  }
}
