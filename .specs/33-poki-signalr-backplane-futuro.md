# Prompt 33 — Backplane SignalR com Redis (futuro)

## Status

Implementado de forma opcional junto com os Prompts 30–32. A ativação permanece
desligada por padrão nos `appsettings.*.json`.

## Objetivo futuro

Distribuir grupos e mensagens dos hubs `/signalr-match` e `/signalr-network` entre múltiplas instâncias da API usando o provider oficial `Microsoft.AspNetCore.SignalR.StackExchangeRedis`.

## Pré-requisitos operacionais

- Provider `ICacheManager`/EAF validado em produção.
- Redis compartilhado com TLS, autenticação e capacidade dimensionada.
- Teste de duas ou mais instâncias concluído.
- Load balancer com WebSockets habilitado.
- Métricas de conexões, Pub/Sub, latência e falhas disponíveis.
- Estratégia de channel prefix por ambiente/tenant definida.

## Implementação entregue

- Pacote `Microsoft.AspNetCore.SignalR.StackExchangeRedis` compatível com `net10.0`.
- `AddStackExchangeRedis` somente quando `RedisCache:IsEnabled` e
  `SignalR:Backplane:IsEnabled` forem verdadeiros.
- `ChannelPrefix` configurável e separado do cache.
- Presença continua usando `ICacheManager`; o backplane é transporte separado.

Ainda requer validação de runtime com duas instâncias e Redis compartilhado antes
de habilitar em produção.

## Não fazer neste prompt

- Implementar Pub/Sub próprio.
- Usar `ICacheManager` como transporte de mensagens.
- Persistir payloads de signaling.
- Prometer entrega exatamente uma vez.
- Remover a compatibilidade com clientes atuais.

## Critérios de aceite futuros

- Mensagem enviada na instância A chega a conexões do grupo na instância B.
- Falha do backplane é observável e não vaza segredo.
- Prefixos e databases não colidem com leaderboard/catálogo.
- Testes de carga e reconexão documentam limites.
