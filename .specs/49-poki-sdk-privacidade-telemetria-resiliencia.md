# 49 — SDK: privacidade, telemetria e resiliência

> **Status:** Especificação para execução
> **Base:** Specs 39, 40 e 68
> **Prioridade:** P1
> **Dependências:** contratos atuais do GameHub; evolução do EAF não obrigatória

## Objetivo

Oferecer ao jogo um SDK seguro, versionado e resiliente sem expor detalhes
internos do GameHub/EAF.

## API pública

```ts
interface GameHubSdk {
  getCapabilities(): Promise<GameHubCapabilities>;
  getPrivacyPolicy(): Promise<PrivacyPolicy>;
  setTelemetryConsent(input: TelemetryConsent): Promise<void>;
  measure(input: MeasureInput): Promise<void>;
  reportPlayer(input: ReportPlayerInput): Promise<void>;
  blockPlayer(userId: number): Promise<void>;
  unblockPlayer(userId: number): Promise<void>;
}
```

## Privacidade

- consentimento explícito para telemetria não essencial;
- versão da política armazenada junto ao consentimento;
- ausência de consentimento não bloqueia gameplay essencial;
- telemetria não contém chat, token, e-mail, IP ou claims;
- `postMessage` aceita somente origens configuradas;
- erros para o iframe contêm `code`, `message`, `retryable` e `correlationId`;
- capabilities são filtradas por jogo, tenant e ambiente.

## Telemetria

- cada evento recebe `clientEventId`;
- lote opcional com limite de tamanho e flush periódico;
- retry com backoff e limite de tentativas;
- fila em memória pode ser descartada no unload;
- eventos de inspector são separados de produção;
- métricas do SDK medem sucesso, falha, latência e descarte.

## Resiliência

- token expirado dispara refresh uma única vez;
- `401` após refresh retorna `not_authenticated`;
- `429` respeita `Retry-After`;
- reconexão SignalR remove handlers antigos;
- timeout não repete operações não idempotentes;
- feature desabilitada retorna `feature_disabled`;
- SDK antigo ignora capabilities desconhecidas.

## Testes

- consentimento aceito, recusado e revogado;
- origem não autorizada;
- token expirado;
- retry e backoff;
- payload acima do limite;
- eventos duplicados;
- reconexão e limpeza de subscriptions;
- compatibilidade de protocolo.

## Critérios de aceite

1. O jogo nunca recebe dados internos do usuário ou da infraestrutura.
2. Telemetria opcional respeita consentimento.
3. Eventos repetidos não duplicam métricas.
4. Jogos existentes continuam funcionando com capabilities desligadas.
5. Contratos TypeScript e exemplos ficam versionados.
