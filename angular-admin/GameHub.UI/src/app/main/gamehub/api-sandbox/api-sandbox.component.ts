import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppConsts } from '@shared/AppConsts';

interface ApiOperation {
  id: string;
  path: string;
  method: string;
  summary: string;
  description: string;
  tags: string[];
  group: string;
  parameters: any[];
  hasBody: boolean;
  bodyValue: string;
  paramValues: { [name: string]: string };
  response?: { status: number; body: any; error?: string };
}

@Component({
  standalone: false,
  selector: 'app-gamehub-api-sandbox',
  templateUrl: './api-sandbox.component.html',
  animations: [appModuleAnimation()],
})
export class ApiSandboxComponent implements OnInit {
  loading = true;
  swaggerAvailable = false;
  errorMessage = '';
  baseUrl = '';
  swaggerJsonUrl = '';
  groups: string[] = [];
  operations: ApiOperation[] = [];
  authToken = '';

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

  constructor(private readonly http: HttpClient) {}

  ngOnInit(): void {
    this.baseUrl = (AppConsts.remoteServiceBaseUrl || '').replace(/\/+$/, '');
    this.swaggerJsonUrl = `${this.baseUrl}/swagger/v1/swagger.json`;

    this.http.get(this.swaggerJsonUrl, { observe: 'response' }).subscribe({
      next: response => {
        if (response.status === 200 && response.body) {
          this.loadSpec(response.body);
        } else {
          this.tryLocalSpec();
        }
      },
      error: () => this.tryLocalSpec(),
    });
  }

  private tryLocalSpec(): void {
    this.http.get('assets/api-sandbox/swagger.json', { observe: 'response' }).subscribe({
      next: response => {
        if (response.status === 200 && response.body) {
          this.loadSpec(response.body);
        } else {
          this.setFallback();
        }
      },
      error: () => this.setFallback(),
    });
  }

  private setFallback(): void {
    this.loading = false;
    this.swaggerAvailable = false;
    this.errorMessage = 'Swagger is offline in this environment.';
  }

  private loadSpec(spec: any): void {
    this.operations = this.buildOperations(spec);
    this.groups = [...new Set(this.operations.map(o => o.group))].sort();
    this.swaggerAvailable = this.operations.length > 0;
    this.loading = false;
    this.errorMessage = this.swaggerAvailable ? '' : 'Nenhuma operação encontrada no documento Swagger.';
  }

  private buildOperations(spec: any): ApiOperation[] {
    const paths = spec?.paths || {};
    const operations: ApiOperation[] = [];

    for (const path of Object.keys(paths)) {
      const pathItem = paths[path];
      for (const method of ['get', 'post', 'put', 'delete', 'patch', 'head', 'options']) {
        const op = pathItem[method];
        if (!op) {
          continue;
        }

        const allParams = [...(pathItem.parameters || []), ...(op.parameters || [])];
        const tags = op.tags?.length ? op.tags : ['default'];
        const hasBody = /^(post|put|patch)$/i.test(method) && !!op.requestBody;

        operations.push({
          id: `${method}-${path}`,
          path,
          method: method.toUpperCase(),
          summary: op.summary || '',
          description: op.description || '',
          tags,
          group: tags[0],
          parameters: allParams,
          hasBody,
          bodyValue: hasBody ? '{}' : '',
          paramValues: {},
        });
      }
    }

    return operations.sort((a, b) => a.group.localeCompare(b.group) || a.path.localeCompare(b.path));
  }

  groupOperations(group: string): ApiOperation[] {
    return this.operations.filter(o => o.group === group);
  }

  curlCommand(op: ApiOperation): string {
    const params = op.parameters.filter(p => p.in === 'path' || p.in === 'query');
    let url = `${this.baseUrl}${op.path}`;

    for (const p of params) {
      const value = op.paramValues[p.name] || `{${p.name}}`;
      url = url.replace(`{${p.name}}`, encodeURIComponent(value));
    }

    const query = params
      .filter(p => p.in === 'query' && op.paramValues[p.name])
      .map(p => `${encodeURIComponent(p.name)}=${encodeURIComponent(op.paramValues[p.name])}`)
      .join('&');

    if (query) {
      url += `?${query}`;
    }

    const headers: string[] = ["-H 'Content-Type: application/json'"];
    if (this.authToken) {
      headers.push(`-H 'Authorization: Bearer ${this.authToken}'`);
    }

    const bodyPart =
      op.hasBody && op.bodyValue
        ? ` \\
  -d '${op.bodyValue.replace(/'/g, "'\\''")}'`
        : '';

    return `curl -X ${op.method} ${url} \\
  ${headers.join(' \\\n  ')}${bodyPart}`;
  }

  tryOperation(op: ApiOperation): void {
    let url = `${this.baseUrl}${op.path}`;
    const pathParams = op.parameters.filter(p => p.in === 'path');
    for (const p of pathParams) {
      url = url.replace(`{${p.name}}`, encodeURIComponent(op.paramValues[p.name] || ''));
    }

    const query = op.parameters
      .filter(p => p.in === 'query' && op.paramValues[p.name])
      .map(p => `${encodeURIComponent(p.name)}=${encodeURIComponent(op.paramValues[p.name])}`)
      .join('&');
    if (query) {
      url += `?${query}`;
    }

    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const requestHeaders = this.authToken ? headers.set('Authorization', `Bearer ${this.authToken}`) : headers;
    const body = op.hasBody ? op.bodyValue : undefined;

    this.http
      .request(op.method, url, {
        headers: requestHeaders,
        body,
        observe: 'response',
      })
      .subscribe({
        next: response => {
          op.response = { status: response.status, body: response.body };
        },
        error: error => {
          op.response = { status: error.status || 0, body: error.error, error: error.message };
        },
      });
  }

  setParamValue(op: ApiOperation, name: string, value: string): void {
    op.paramValues[name] = value;
  }
}
