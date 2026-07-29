# Plano de correção dos erros GameHub (admin / API)

## Contexto

Durante testes no ambiente de produção (`gamehub-admin.afonsoft.dev` -> `gamehub-api.afonsoft.dev`) foram observados:

1. **SignalR / CORS**: `POST https://gamehub-api.afonsoft.dev/signalr-chat/negotiate?negotiateVersion=1` retorna `504 Gateway Timeout (de service worker)` e o browser bloqueia com:
   > *Request header field x-signalr-user-agent is not allowed by Access-Control-Allow-Headers in preflight response.*
2. **Fonte WOFF2**: `Inter-roman.var.ed4cd0c7c0b73726.woff2?v=3.19` falha com:
   > *Failed to decode downloaded font: ... OTS parsing error: Failed to convert WOFF 2.0 font to SFNT*
3. **m-switch duplicado**: componente `m-switch` com label (`.m-switch-label`) renderiza dois knobs/toggles.
4. **Aria-hidden**: aviso no console sobre `aria-hidden` no `div.modal fade` enquanto descendente (`button.close`) mantém foco.

## Análise / causa raiz

### 1. CORS — `X-SignalR-User-Agent` não permitido

A política CORS é registrada por `AddEafCors` (EAF). A lista de headers permitidos não inclui o header customizado `X-SignalR-User-Agent` enviado pelo cliente SignalR (`@microsoft/signalr` 7.x). O preflight `OPTIONS` retorna `204`, mas sem o header na resposta, o browser cancela a requisição real. O erro `504 de service worker` é consequência: o SW não consegue obter a resposta válida e devolve timeout.

**Correção GameHub (hotfix local até nova versão EAF):** pós-configurar a política CORS após `AddEafCors`, adicionando `X-SignalR-User-Agent`.

**Correção EAF (template):** incluir o header nativamente em `EafCorsConfiguration.WithHeaders`.

### 2. Fonte WOFF2 — rewrite IIS servindo `index.html` para assets faltantes

A fonte `Inter-roman.var.*.woff2` é gerada pelo build Angular a partir do tema `primeng` (`lara-light-blue`) e colocada na raiz do `dist` com hash. O `web.config` possui a regra:

```xml
<add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true"/>
```

Se o arquivo com hash antigo não existir no servidor (deploy parcial, cache do SW com build anterior), a regra reescreve a URL para `/` (`index.html`), e o browser tenta parsear HTML como WOFF2.

**Correção:** adicionar condição negativa no rewrite para extensões de assets estáticos (`woff2`, `woff`, `ttf`, `eot`, `js`, `css`, `png`, `jpg`, `svg`, etc.), evitando servir `index.html` para arquivos de fonte/imagens.

### 3. m-switch duplicado — label span herdando estilos do toggle

O tema Metronic usa:

```css
.m-switch input:empty ~ span { ... }
.m-switch input:empty ~ span:before,
.m-switch input:empty ~ span:after { ... }
```

O seletor `~ span` pega **ambos** os spans irmãos do `input`:

```html
<input type="checkbox">
<span></span>          <!-- knob real -->
<span class="m-switch-label">Texto</span>  <!-- label -->
```

Por isso o span de label também recebe `:before`/`:after`, criando um segundo knob visual.

**Correção:** adicionar override em `styles.css` para `.m-switch input:empty ~ span.m-switch-label` removendo `float`, `position`, `width` e escondendo os pseudo-elementos (`content: none`).

### 4. aria-hidden em modal aberto

Os templates de modal declaram `aria-hidden="true"` no `div` raiz (`class="modal fade"`). Quando o modal é exibido, o `button.close` dentro dele recebe foco, e o browser emite o aviso de acessibilidade.

**Correção:** remover `aria-hidden="true"` dos `div` raiz dos modais. O `bsModal` do ngx-bootstrap já gerencia o atributo dinamicamente.

## Arquivos a alterar (gamehub)

| Erro | Arquivo(s) |
|------|------------|
| CORS SignalR | `Api/src/GameHub.Web.Host/Startup/Startup.cs` (+ extensão auxiliar) |
| CORS SignalR (teste) | `Api/test/GameHub.Tests/Middleware/CorsConfiguration_Tests.cs` |
| WOFF2 / assets | `angular-admin/GameHub.UI/src/web.config` |
| m-switch | `angular-admin/GameHub.UI/src/assets/common/styles/styles.css` |
| aria-hidden | `angular-admin/GameHub.UI/src/app/**/ *-modal.component.html` |

## Arquivos EAF (template)

Ver especificação separada: [2026-07-29-eaf-template-sync.md](./2026-07-29-eaf-template-sync.md).

## Simulação local já feita

- `ng build --configuration=production` gera corretamente a fonte `Inter-roman.var.*.woff2` no `dist`.
- Página de teste `mswitch-test.html` carregando `style.bundle.css` + `styles.css` reproduziu o m-switch duplicado.
- Aplicação do override CSS na página de teste eliminou o duplicado.

## Verificação pós-correção

1. `dotnet test Api/test/GameHub.Tests/GameHub.Tests.csproj --filter "FullyQualifiedName~CorsConfiguration"` passar com novo caso `X-SignalR-User-Agent`.
2. `npx ng build --configuration=production` concluir sem erros.
3. Página de teste do `m-switch` mostrar apenas um toggle por item.
4. `web.config` validado (sem erros de sintaxe XML / IIS Rewrite).
5. Logs do console não devem mais exibir erro CORS para `signalr-chat/negotiate`.
