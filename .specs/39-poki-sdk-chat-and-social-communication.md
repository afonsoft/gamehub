# 39 — SDK: chat e comunicação social

> **Status:** Especificação para execução
> **Base:** EAF Chat (`Eaf.Middleware.Web.Core.SignalR.Chat.ChatHub`), `/signalr-chat` e `GameplayBridgeService`
> **Objetivo:** permitir que jogos usem chat autenticado e moderado sem duplicar a infraestrutura de mensagens já fornecida pelo EAF.

## 1. Decisão arquitetural

O EAF permanece responsável por:

- `ChatMessage` e persistência de mensagens;
- `IChatMessageManager` e `IChatAppService`;
- `ChatHub` e entrega SignalR;
- leitura, não lida e histórico;
- comunicação entre usuários e grupos.

O GameHub deve fornecer somente:

- uma fachada SDK segura para jogos;
- autorização contextual por jogo/partida;
- adaptação de mensagens para `postMessage`;
- rate limit e limites de payload específicos do jogo;
- moderação e política de retenção específicas da plataforma;
- documentação e exemplos de integração.

Não criar uma segunda entidade `GameChatMessage` nem um segundo hub para repetir
o armazenamento do EAF.

## 2. Modos de chat

### 2.1 Chat privado

Mensagem entre o jogador atual e outro usuário permitido pelo contexto do jogo.

### 2.2 Chat de grupo

Mensagem para o grupo associado a uma sala, partida ou equipe. A autorização do
grupo deve ser validada pelo backend; o jogo não pode escolher arbitrariamente
um `receiverGroup`.

### 2.3 Chat da partida

Quando uma partida multiplayer estiver ativa, o SDK deve usar um identificador
de conversa derivado de `matchId`. Participantes que saírem da partida perdem a
permissão de envio, conforme a política configurada.

## 3. API pública do SDK

Adicionar uma fachada `GameHubChatClient` ao `GameplayBridgeService`, sem expor
objetos internos do SignalR:

```ts
interface GameHubChatClient {
  connect(context: ChatContext): Promise<ChatConnectionResult>;
  disconnect(): Promise<void>;
  send(input: SendChatMessageInput): Promise<ChatMessageResult>;
  getHistory(input: ChatHistoryInput): Promise<ChatHistoryResult>;
  markRead(messageIds: string[]): Promise<void>;
  onMessage(callback: (message: ChatMessage) => void): () => void;
  onPresenceChanged(callback: (change: ChatPresenceChange) => void): () => void;
}
```

O protocolo `postMessage` do bridge deve suportar:

- `chatConnect`;
- `chatDisconnect`;
- `chatSend`;
- `chatHistory`;
- `chatMarkRead`;
- `chatMessage` (evento do host para o jogo);
- `chatPresenceChanged` (evento do host para o jogo).

Toda operação assíncrona deve responder com `requestId`, `data` ou `error`,
seguindo o protocolo atual do bridge.

## 4. Contratos mínimos

```ts
interface ChatContext {
  gameId: string;
  matchId?: string;
  conversationId?: string;
}

interface SendChatMessageInput {
  conversationId: string;
  text: string;
  clientMessageId: string;
}

interface ChatMessage {
  id: string;
  conversationId: string;
  senderUserId: number;
  senderName: string;
  text: string;
  sentAt: string;
  readState: 'read' | 'unread';
}
```

Regras:

- `text` deve ter tamanho máximo definido pelo servidor, inicialmente 500
  caracteres;
- `clientMessageId` deve permitir deduplicação;
- o servidor normaliza Unicode e remove controle não imprimível;
- o servidor nunca aceita `senderUserId` ou `senderName` enviados pelo jogo;
- timestamps retornados pelo backend devem ser UTC em ISO 8601.

## 5. Conexão e resiliência

- Usar `/signalr-chat` do EAF com `accessTokenFactory` já existente.
- Usar reconexão automática com backoff.
- Não tentar conexão automática sem contexto de jogo válido.
- Ao reconectar, recuperar mensagens desde o último cursor conhecido.
- Mensagens pendentes devem ser marcadas como `failed` após timeout; não repetir
  indefinidamente.
- `disconnect()` deve remover handlers e liberar a conexão.
- O chat não pode bloquear `gameLoadingFinished`, gameplay ou ad breaks.

## 6. Segurança, privacidade e moderação

- Aplicar autenticação do jogador e tenant em toda operação.
- Usuário anônimo deve ter chat desabilitado por padrão.
- Validar que o usuário participa do jogo, grupo ou partida informada.
- Aplicar rate limit por usuário, jogo e conversa.
- Bloquear spam repetitivo e payloads acima do limite.
- Reutilizar `ProfanityFilter`/moderação de UserContent quando aplicável.
- Permitir reportar uma mensagem com `messageId` e motivo.
- Nunca enviar token JWT, e-mail, IP ou dados de moderação ao jogo.
- Definir retenção e exclusão conforme política de privacidade do GameHub.

## 7. UX mínima no host

O host deve fornecer uma UI opcional para:

- estado conectado, reconectando e offline;
- contador de mensagens não lidas;
- envio bloqueado durante rate limit;
- erro amigável de moderação;
- reportar, silenciar e bloquear usuário;
- ocultar chat durante anúncios e telas de consentimento.

O jogo pode fornecer UI própria, mas deve receber somente os contratos públicos
do SDK e respeitar as decisões de moderação do host.

## 8. Testes

- `GameplayBridgeService`:
  - conecta com token válido;
  - rejeita contexto sem `gameId`;
  - envia `chatSend` com `requestId`;
  - encaminha `chatMessage`;
  - reconecta sem duplicar handlers;
  - limpa conexão em `disconnect`.
- Backend/EAF integration:
  - usuário sem participação não envia para a conversa;
  - mensagem acima do limite é rejeitada;
  - `clientMessageId` é idempotente;
  - rate limit bloqueia spam;
  - histórico respeita tenant e conversa.
- Segurança:
  - não registrar texto integral em logs;
  - não devolver claims sensíveis;
  - não permitir bypass de moderação via `postMessage`.

## 9. Fora do escopo

- Chat de voz ou vídeo.
- Upload de imagens, arquivos, links ou rich text.
- Mensagens anônimas persistidas.
- Criar uma implementação paralela ao EAF.
- Garantir entrega de chat entre múltiplas instâncias sem o backplane SignalR
  configurado.
