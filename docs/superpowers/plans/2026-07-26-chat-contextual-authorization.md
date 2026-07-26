# Plano: autorização contextual do chat do SDK

## Objetivo

Manter o EAF como fonte de verdade para `ChatMessage`, persistência e `/signalr-chat`, mas impedir que um jogo envie mensagens para usuários ou partidas sem participação autorizada.

## Sequência

1. Adicionar contratos de aplicação para envio contextual e resultado idempotente.
2. Validar `gameId`, `matchId`, tenant, usuário autenticado e participação ativa antes do envio.
3. Usar `IChatMessageManager`/`ChatMessage` do EAF para persistência e entrega, sem entidade ou hub paralelo.
4. Derivar identidade do usuário pelo `AbpSession`/token; ignorar identidade enviada pelo iframe.
5. Implementar deduplicação por `clientMessageId` com cache distribuído e documentar a janela de retenção.
6. Alterar o bridge para enviar mensagens pelo endpoint contextual e manter o EAF ChatHub para eventos em tempo real.
7. Cobrir autorização, limite de texto, tenant isolation, match encerrada e repetição de request.
8. Atualizar o guia do usuário e o log de execução.

## Verificação

```bash
dotnet build Api/GameHub.sln --no-restore
dotnet test Api/GameHub.sln --no-build
cd angular && npm run build
git diff --check
```

## Limites explícitos

- A entrega em tempo real continua usando o `/signalr-chat` do EAF.
- A deduplicação usa a janela configurada do cache; exatamente-once após expiração exige suporte futuro no EAF.
- Conversas de partida só incluem participantes autenticados e ativos; usuários anônimos ficam bloqueados.
