# Sincronização do template EAF — Chat bar theme

## Contexto
O `chat-bar` do admin GameHub usava cores e classes fixas que não acompanhavam o tema Metronic/EAF (`primary #FF7020`, `success #34bfa3`, etc.). Foram ajustados o CSS e o HTML do componente para alinhar ao tema e ao layout do template EAF.

## Alterações no GameHub

### Arquivos
- `angular-admin/GameHub.UI/src/app/shared/layout/chat/chat-bar.component.html`
- `angular-admin/GameHub.UI/src/app/shared/layout/chat/chat-bar.component.css`

### HTML
- Substituído `style="width: 96%"` por `class="chat-shell"`.
- Adicionadas classes do template EAF: `chat-conversation`, `chat-messages-scroll`, `chat-composer`, `chat-attachment-actions`, `chat-send-actions`.
- Substituído `style="color: #ff7020"` por `class="text-primary"` nos nomes dos remetentes.
- Adicionado `text-light` no ícone do pino.

### CSS
- Header `.bs-canvas-header` com `background-color: var(--primary, #FF7020)` e texto/brancos, garantindo cor do tema mesmo quando `style.bundle.css` não carrega (modo dev).
- Cores das bolhas de mensagem com `bg-light-primary` (laranja 10%) e `bg-light-success` (verde 10%).
- Cores dos status dots `label-success` / `label-secondary`.
- Previews de arquivo (`chat-file-preview`) e link (`chat-link-message`) com a cor primária do tema.
- Botões de anexo (`chat-attachment-actions .btn`) com acento primário.
- Reset do `.card` escopado a `#chatSideRight`.
- Layout flex (`chat-shell`, `chat-conversation`, `chat-messages-scroll`) para ocupar a altura disponível.

## Alterações no template EAF

### Arquivos
- `Templates/Angular/Eaf.ProjectName.UI/src/app/shared/layout/chat/chat-bar.component.html`
- `Templates/Angular/Eaf.ProjectName.UI/src/app/shared/layout/chat/chat-bar.component.css`

### HTML
- Ícone do pino com `text-light`.
- `style="color: #ff7020"` trocado por `class="text-primary"`.

### CSS
- `.chat-context-banner` ajustado para tons da cor primária (`rgba(255,112,32,0.08)`).
- `.chat-file-preview` e `.chat-link-message` com a cor primária do tema.
- Header fallback, status dots, empty state e botões de anexo iguais aos do GameHub.
- Reset do `.card` escopado a `#chatSideRight`.

## Pontos de atenção para projetos derivados do EAF
- O `chat-bar` deve sempre usar `var(--primary, #FF7020)` para não quebrar quando o tema Metronic não estiver carregado.
- As classes de layout (`chat-shell`, `chat-conversation`, `chat-messages-scroll`) já estão no template; mantê-las evita regressão no comportamento flex do painel.
- O reset global `.card { box-shadow:none; border:none; }` deve ser escopado ao `#chatSideRight` para não afetar outros cards do admin.
