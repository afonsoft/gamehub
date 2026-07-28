import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppConsts } from '@shared/AppConsts';

@Component({
  standalone: false,
  selector: 'app-gamehub-api-sandbox',
  templateUrl: './api-sandbox.component.html',
  animations: [appModuleAnimation()],
})
export class ApiSandboxComponent implements OnInit {
  swaggerUrl: SafeResourceUrl | null = null;
  swaggerJsonUrl: string = '';
  swaggerAvailable = false;
  checking = true;
  errorMessage = '';

  readonly examples = {
    auth: `curl -X POST ${AppConsts.remoteServiceBaseUrl}/api/TokenAuth/Authenticate \\
  -H 'Content-Type: application/json' \\
  -d '{"userNameOrEmailAddress":"admin","password":"***"}'`,
    games: `curl ${AppConsts.remoteServiceBaseUrl}/api/services/app/GameCatalog/GetGames`,
    gameplay: `curl -X POST ${AppConsts.remoteServiceBaseUrl}/api/services/app/Gameplay/StartSession \\
  -H 'Authorization: Bearer {token}' \\
  -H 'Content-Type: application/json' \\
  -d '{"gameId":"{gameId}","deviceType":"Desktop"}'`,
  };

  constructor(
    private readonly http: HttpClient,
    private readonly sanitizer: DomSanitizer,
  ) {}

  ngOnInit(): void {
    const baseUrl = (AppConsts.remoteServiceBaseUrl || '').replace(/\/+$/, '');
    this.swaggerJsonUrl = `${baseUrl}/swagger/v1/swagger.json`;
    const swaggerUiUrl = `${baseUrl}/swagger`;
    this.swaggerUrl = this.sanitizer.bypassSecurityTrustResourceUrl(swaggerUiUrl);

    this.http.get(this.swaggerJsonUrl, { observe: 'response' })
      .subscribe({
        next: response => {
          this.swaggerAvailable = response.status === 200;
          this.checking = false;
        },
        error: () => {
          this.swaggerAvailable = false;
          this.checking = false;
          this.errorMessage = 'Swagger is offline in this environment.';
        },
      });
  }
}
