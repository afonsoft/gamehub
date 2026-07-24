# 21 — Quality & Compliance (inspirado nos requisitos da Poki)

## Fonte

- https://sdk.poki.com/new-requirements
- https://sdk.poki.com/sdk-documentation

## Objetivo

Aplicar ao GameHub os requisitos de qualidade e conformidade que garantem que jogos publicados funcionem em qualquer dispositivo, respeitem privacidade e entreguem uma experiência sem fricção.

## 21.1 — Device & Aspect Ratio

- O jogo deve suportar desktop, mobile e tablet.
- Em mobile, ocupar tela cheia em portrait e/ou landscape (ou ambos).
- Aspect ratio alvo 16:9; canvas sugeridos 640x360, 836x470, 1031x580.
- O `game-frame` e o SDK devem reportar `Orientation` e `DeviceType` corretamente.

### Tarefas

1. Validar no `GameBuildPackageValidator` que o build responde a `ResizeObserver` / `window.resize`.
2. Adicionar warning no Inspector quando o jogo não informa suporte a mobile ou tablet.
3. Incluir metadados de orientação na página do jogo e no `GameDetailDto`.

## 21.2 — Incognito / No LocalStorage

- Jogos devem funcionar no modo anônimo do navegador.
- `localStorage` deve estar envolto em `try/catch` no frontend (já parcial no `PlayerService`).
- Backend não deve depender de localStorage; usar `deviceId` + sessão como fallback.

### Tarefas

1. Garantir que `GameplayBridgeService` e `PlayerService` capturam exceções de `localStorage`.
2. Criar teste E2E/Unitário de execução em modo anônimo.
3. Salvar preferências no backend para usuários autenticados; para anônimos, usar `deviceId` somente para vota/report.

## 21.3 — External Requests & Privacy

- Por padrão, bloquear requests de terceiros vindos do iframe (CSP).
- Multiplayer e analytics de terceiros precisam de declaração de privacidade (`PrivacyPolicy`) aprovada.
- Google Analytics bloqueado por padrão.

### Tarefas

1. Expandir `ContentSecurityPolicyMiddleware` para `frame-src` e `connect-src` por `gameId`/`PrivacyPolicy.ExternalRequests`.
2. Adicionar flag `AllowsExternalRequests` em `PrivacyPolicy`.
3. Criar endpoint de declaração de privacidade que o jogo deve expor (`/api/services/app/Privacy/GetForGame`).

## 21.4 — No Splash Screens / Outgoing Links

- Remover splash screens e links externos dos jogos publicados.
- Permitir logo do estúdio na tela de loading.

### Tarefas

1. No `InspectorAppService.RunBuildValidationAsync` verificar:
   - presença de `index.html`;
   - ausência de arquivos de splash screen óbvios (`splash.*`, `intro.*`);
   - ausência de links `window.open`, `location.href` ou tags `<a href>` para domínios externos.
2. Gerar warning quando encontrado.

## 21.5 — File Size & Clean Build

- Download inicial < 8 MB (preferencial).
- Remover código de debug, ferramentas de dev e artefatos de teste.

### Tarefas

1. Validar tamanho do build no upload (já existe 100 MB; adicionar warning se > 8 MB).
2. Lista de artefatos proibidos: `*.map`, `*.log`, `node_modules/`, `test/`, `__tests__/`.
3. Integrar ao Inspector como "Build Size" e "Clean Build" warnings.

## 21.6 — SDK Events Quality

Regras da Poki:

- `gameplayStart` não pode ser disparado duas vezes seguidas.
- `gameplayStop` não pode ser disparado duas vezes seguidas.
- `gameplayStart` deve ocorrer no primeiro input do jogador, não no load.
- `gameplayStop` deve ocorrer em pausas, menus, fim de fase, cutscenes.
- Nenhum evento SDK deve disparar durante midrolls/rewarded videos.
- `commercialBreak` só pode disparar ao sair de uma pausa e voltar ao gameplay.

### Tarefas

1. `SdkEventLog` armazenar todos os eventos por sessão.
2. `InspectorAppService.ValidateSessionAsync` verificar regras acima e retornar warnings.
3. `GameplayBridgeService` já trata state machine; adicionar log local dos eventos e enviar batch para `/api/services/app/Inspector/RecordSdkEvent`.

## 21.7 — No Ad Block Prevention

- Jogo deve ser jogável mesmo com ad block.
- Não exibir mensagens de "desative o ad block".

### Tarefas

1. Confirmar que `AdBreakResult.AdBlocked` retorna `false` para completed no `FakeAdProvider`.
2. `GameplayBridgeService` não mostrar UI de erro quando `adBlocked`.
3. Teste de unidade para `commercialBreakRequested` com `adBlocked = true`.

## 21.8 — Quality Guidelines Checklist

Criar entidade `QualityChecklist` (ou reutilizar `InspectorReport`) com itens:

- [ ] SDK events corretos
- [ ] Dispositivos suportados
- [ ] Aspect ratio 16:9
- [ ] < 8 MB
- [ ] Sem external requests não autorizados
- [ ] Sem splash/outgoing links
- [ ] Funciona em incognito
- [ ] Sem ad block prevention

### Tarefas

1. Backend `IGameQualityAppService` com `GetChecklistAsync` e `UpdateChecklistAsync`.
2. Tela no admin mostrando checklist e bloqueando publicação se itens críticos falharem.

## Dependências

- 19.10 (Inspector v2) fornece a base para validações.
- 19.12 (Privacy/UGC/Performance) fornece `PrivacyPolicy` e modo anônimo.

## Critérios de aceitação

- Build > 8 MB gera warning, não rejeição.
- Inspector reporta todos os warnings acima.
- Jogo com checklist crítico incompleto não pode ser publicado.
- Todos os novos métodos cobertos por testes.
- `dotnet build`, `dotnet test` e `npm run build` passam.
