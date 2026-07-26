# 40 — SDK: capacidades complementares para jogos

> **Status:** Especificação para execução
> **Base:** Specs 22, 28, 35 e 39
> **Objetivo:** organizar funcionalidades do SDK que devem compartilhar autenticação, permissões, resiliência e observabilidade com o chat.

## 1. Presença e perfil social

- `getUser()` deve retornar somente o perfil público autorizado.
- `getPresence(userIds)` deve retornar `online`, `away` ou `offline`.
- `onPresenceChanged` deve emitir alterações relevantes sem polling agressivo.
- O jogo não deve descobrir e-mail, tenant ou identificadores internos.

## 2. Notificações in-game

- `getNotifications()` lista notificações do jogador relacionadas ao jogo.
- `markNotificationRead(notificationId)` altera o estado no backend.
- `onNotification` entrega novas notificações enquanto o jogo estiver aberto.
- Notificações precisam de categoria, severidade, timestamp e deep link seguro.
- O host pode silenciar notificações durante gameplay ou anúncios.

## 3. Amigos e convites

- `invitePlayer(userId, matchId)` cria convite somente para partidas permitidas.
- `acceptInvite(inviteId)` valida expiração, jogo e autorização.
- Convites devem usar token opaco e expiração curta.
- Não implementar lista global de amigos nesta etapa; iniciar com relações
  limitadas ao contexto de jogo/partida.

## 4. Telemetria do SDK

- `measure(category, name, value)` continua separado de mensagens de chat.
- Adicionar correlação por `sessionId`, `gameId`, `buildId` e `matchId`.
- Agregar eventos antes do envio para reduzir custo.
- Nunca registrar texto de chat, tokens ou conteúdo de mensagens em telemetria.
- O jogador deve poder optar por não participar de telemetria não essencial.

## 5. Contrato comum de erros

Todas as capacidades devem usar:

```ts
interface SdkError {
  code: string;
  message: string;
  retryable: boolean;
  correlationId?: string;
}
```

Códigos mínimos:

- `not_authenticated`;
- `not_authorized`;
- `feature_disabled`;
- `rate_limited`;
- `invalid_context`;
- `temporarily_unavailable`;
- `validation_failed`.

## 6. Feature flags e compatibilidade

- Cada capacidade deve ser habilitada por jogo/tenant.
- O SDK deve responder `feature_disabled` sem quebrar jogos antigos.
- A versão do protocolo deve ser informada no handshake.
- Métodos novos devem ser aditivos e não alterar o significado dos eventos
  existentes.

## 7. Critérios de aceite

- Chat, presença e notificações compartilham autenticação e reconexão.
- Cada nova função possui documentação de payload e exemplos.
- Jogos antigos continuam funcionando quando as features estão desligadas.
- Logs e métricas não contêm PII desnecessária.
- Testes cobrem feature flag desligada, token expirado, reconexão e autorização.
