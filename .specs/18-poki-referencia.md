# 18 - Referência Poki para o GameHub

> **Status:** Referência  
> **Fontes:** site `https://poki.com/` e documentação `https://sdk.poki.com/`  
> **Nota:** Este spec não copia marca, layout, assets ou jogos da Poki. Mapeia conceitos e regras observadas para servir de inspiração no GameHub.

---

## 1. Visão geral da Poki

A Poki é descrita como a maior plataforma de jogos web do mundo, com mais de 100 milhões de usuários ativos mensais e mais de 1.700 jogos HTML5/WebGL. Todo o conteúdo é gratuito, acessível em desktop, tablet e mobile, sem downloads nem pop-ups.

Pontos-chave observados no site e documentação:

- Catálogo curado com destaques por semana, top trending, exclusivos web e jogos licenciados.
- Categorias extensas e voltadas a estados de humor do jogador (ex.: "Action Games", "Puzzle Games", "2 Player Games", "Cozy Games").
- Página de jogo com metadados claros: título, desenvolvedor, curtir/não curtir, reportar bug, tela cheia, controles, descrição, jogos relacionados e categorias.
- Contas opcionais para salvar progresso e favoritos entre dispositivos.
- Portal do desenvolvedor (Poki for Developers) com ciclo de testes, análises, versões, playtests, thumbnails e métricas.
- SDK leve focado em eventos de carregamento, gameplay, breaks comerciais e recompensados.
- Requisitos rigorosos de qualidade, performance e privacidade.
- Modelo de receita baseado em exclusividade web e divisão de receita (100% quando o desenvolvedor traz o jogador, 50% quando a Poki traz).

---

## 2. Portal público

### 2.1 Home e descoberta

A home é organizada em seções de descoberta:

- **Popular this week:** jogos em alta na semana.
- **Top free games:** jogos mais jogados no momento.
- **Poki web exclusive & licensed games:** destaques próprios ou licenciados.
- **Categorias por humor/público:** car games, puzzle, .io, 2 player, shooting, etc.
- **Busca:** acesso rápido a qualquer jogo.

Cada seção usa cards com imagem, título e, quando aplicável, nota da comunidade.

### 2.2 Página de jogo

Elementos observados na página de um jogo:

- Título e desenvolvedor.
- Botões de **curtir** e **não curtir** (Like / Dislike).
- **Report a bug** para reportes de problema.
- **Fullscreen** quando habilitado.
- Área de anúncios ("Advertisement") acima e ao lado do jogo.
- Descrição rica com "Show more".
- Seções temáticas (ex.: "Latest World", "Characters", "Gameplay", "Items", "Power ups", "Controls").
- Tabela de preços/informações do jogo quando faz sentido.
- Lista de jogos relacionados e da mesma categoria.
- Categorias/tags associadas.

### 2.3 Contas de jogador

A plataforma permite contas opcionais. Sem conta, o progresso fica em cookies do navegador. Com conta, o progresso é salvo na nuvem e sincronizado entre desktop, tablet e mobile. A autenticação pode usar Apple, Google, Microsoft ou passkey.

---

## 3. Portal do desenvolvedor (Poki for Developers)

### 3.1 Estrutura do time

- **General:** informações de contato e país.
- **Users:** múltiplos usuários com papéis:
  - **Developer:** acesso completo.
  - **Developer Support:** pode subir versões, mas não vê ganhos e métricas de monetização após o lançamento.
- **Billing:** informações de pagamento liberadas após aprovação.

### 3.2 Ciclo de vida do jogo

O desenvolvedor acompanha o jogo por etapas de testes e revisão:

1. Upload de builds.
2. Testes automatizados e manuais.
3. Playtests (gravações de sessões).
4. Poki Inspector (verificação de SDK, scaling, warnings).
5. Revisão final da equipe Poki (Final Review).

Após aprovação, entram métricas de performance.

### 3.3 Funcionalidades do portal

- **Versions:** histórico de builds, upload de novas versões, ação para abrir no Inspector ou Preview.
- **Playtests:** revisão e solicitação de novas gravações de playtest.
- **Settings:** título, descrição sugerida e categorias sugeridas (até quatro).
- **Thumbnails:** upload e atualização de thumbnails. Após publicação, toda mudança passa por revisão.
- **Analytics:** DAU, engagement, earnings, ad performance, player feedback, errors, filtráveis por país e dispositivo.

---

## 4. SDK da Poki

### 4.1 Inicialização

O SDK é carregado via script e inicializado no início do jogo:

```html
<script src="https://game-cdn.poki.com/scripts/v2/poki-sdk.js"></script>
```

```js
PokiSDK.init().then(startLoading).catch(startLoading);
```

### 4.2 Eventos obrigatórios

| Evento | Quando disparar |
|--------|-----------------|
| `gameLoadingFinished()` | Após todos os assets carregarem. |
| `gameplayStart()` | Quando o jogador começa a interagir (primeiro input), despausa ou inicia nível. |
| `gameplayStop()` | Quando o jogo pausa, termina nível, game over, cutscene ou menu. |
| `commercialBreak()` | Em pausas naturais, antes de retomar ao gameplay. |
| `rewardedBreak()` | Quando o jogador escolhe assistir anúncio por recompensa. |

### 4.3 Sequências típicas

- Início: `gameLoadingFinished()` → `commercialBreak()` → `gameplayStart()`
- Morte e reinício: `gameplayStop()` → `commercialBreak()` → `gameplayStart()`
- Morte e revive: `gameplayStop()` → `rewardedBreak()` → `gameplayStart()`
- Pausa/despausa: `gameplayStop()` → `gameplayStart()`

### 4.4 Regras de eventos

- Dois `gameplayStart()` ou dois `gameplayStop()` não podem ser disparados seguidos.
- `gameplayStart()` deve ser no primeiro input, não no load.
- `gameplayStop()` deve ser disparado em qualquer interrupção do gameplay.
- Nenhum evento pode ser disparado durante midrolls ou rewarded videos.
- `commercialBreak()` só deve ser chamado ao sair de uma pausa e voltar ao gameplay.

### 4.5 Contas e cloud saves

- `PokiSDK.login()` abre painel de login.
- `PokiSDK.getUser()` retorna `{ username, avatarUrl }`.
- `PokiSDK.getToken()` retorna JWT de 1 minuto para verificação no backend.
- Cloud saves são automáticos: o SDK monitora `localStorage` e `IndexedDB` e sincroniza mudanças.
- Dados prefixados com `poki_ignore` são ignorados.
- Limite: 1 MB após gzip.

---

## 5. Requisitos e qualidade

### 5.1 Suporte e performance

- Desktop, mobile e tablet.
- Proporção 16:9, com dimensões de referência: 640x360, 836x470, 1031x580.
- Funcionar em modo anônimo/incognito (localStorage deve estar em try/catch).
- Mínimo de 30 FPS, alvo de 60 FPS.
- Funcionar sem travamentos em pelo menos 85% dos usuários.
- Tamanho inicial de download abaixo de 8 MB (recomendado).

### 5.2 Privacidade e segurança

- Bloqueio de requests externos por padrão.
- Jogos multiplayer ou com analytics de terceiros precisam de declaração de privacidade aprovada.
- Google Analytics é bloqueado em todos os casos.
- Remover splash screens e links externos.
- Política de privacidade dentro do jogo quando houver links externos.

### 5.3 UX e conteúdo

- Entrada direta ao gameplay, sem muitas telas.
- Cutscenes e intros devem ser puláveis.
- Controles visuais e instruções claras.
- Suporte a teclado (ESC ou espaço para pausa/resume).
- Controles adaptativos: touch/tablet recebem controles mobile; desktop recebe instruções de teclado.
- Filtro de palavrões para conteúdo gerado por usuários.
- Sem compras internas, anúncios próprios ou comunicação comercial não aprovada.
- Apenas o sistema de anúncios da plataforma é permitido.

### 5.4 Monetização e recompensas

- Mudo automático de áudio durante anúncios.
- Não implementar timers internos de anúncios.
- Uma recompensa por vídeo, sem múltiplos vídeos consecutivos para a mesma recompensa.
- Botão padrão deve ser igual ou maior que o botão de recompensa.
- Botões de recompensa devem usar ícones de vídeo (🎬 ou 🎞️) e não a cor verde.
- Não entregar recompensa quando ad block for detectado.
- Não exibir mensagens de "ad block" customizadas.

### 5.5 Poki Inspector e QA

Ferramenta de teste que permite:

- Carregar o jogo via drag-and-drop ou a partir de uma versão.
- Ver log de eventos do SDK.
- Checar lista de QA por módulos.
- Alternar entre desktop e mobile.
- Testar scaling em dimensões e dispositivos.
- Ver warnings: recursos externos, otimização de imagens, comportamento inesperado do SDK.

---

## 6. Modelos de negócio

### 6.1 Exclusividade web

- Preferência por exclusividade na web (Poki como única plataforma web).
- Discord e YouTube Playables são considerados web e conflitam com a exclusividade.
- Contratos típicos de 5 anos.
- Benefícios: marketing, monetização, divisão de receita.

### 6.2 Divisão de receita

- **100%** para o desenvolvedor quando o jogador chega diretamente (busca, favoritos, redes sociais, comunidade do dev).
- **50%** para cada lado quando a Poki traz o jogador (homepage, promoções, campanhas).

### 6.3 Não exclusivo

- Pagamento fixo único (flat license fee).
- Sem divisão de receita nem marketing adicional.

---

## 7. Mapeamento para o GameHub

### 7.1 Já implementado

- Catálogo com home, busca, filtros por categoria/tag/dispositivo/orientação.
- Página de detalhe do jogo com execução em iframe sandbox.
- SDK/bridge com eventos: `gameLoadingStarted`, `gameLoadingFinished`, `gameplayStart`, `gameplayStop`, `commercialBreak`, `rewardedBreak`, `gameErrorCaptured`, `gameMeasuredEvent`.
- Validação de build (index.html obrigatório, sem executáveis, sha-256, tamanho máximo).
- Moderação, categorias, tags, reports e suspensão.
- Leaderboard via Redis Sorted Sets.
- Painel admin com dashboard, jogos, uploads, revisões, reports e CRUD de categorias/tags.

### 7.2 Conceitos a considerar

- **Página de jogo:** like/dislike, report bug, botão tela cheia, seção de controles, descrição expandida e jogos relacionados.
- **Contas de jogador:** autenticação opcional, salvamento na nuvem, favoritos e progresso entre dispositivos.
- **Sugestões do desenvolvedor:** permitir que o dev sugira até 4 categorias e descrição otimizada para SEO.
- **Revisão de thumbnails:** thumbnail/hero passam por fila de aprovação após o jogo publicado.
- **Poki Inspector interno:** página de QA com checklist, log de eventos, scaling tests e warnings.
- **Métricas de qualidade:** tamanho do build, performance FPS, erros, recursos externos, otimização de imagens.
- **Monetização:** contratos de exclusividade, divisão de receita, ad provider, breaks comerciais e recompensados.
- **Filtro de profanidade:** em UGC, nicknames e comentários.
- **Anonimato/incognito:** SDK e jogos devem tolerar `localStorage` indisponível.
- **Política de privacidade no jogo:** quando o jogo fizer requests externos.

### 7.3 Diferenciação

O GameHub não deve copiar layout, cores, nome, marca, categorias ou jogos da Poki. A referência serve para entender o que funciona em uma plataforma de jogos web madura e adaptar aos objetivos próprios do GameHub.

---

## 8. Links de consulta

- Site: `https://poki.com/`
- SDK HTML5: `https://sdk.poki.com/html5`
- SDK geral: `https://sdk.poki.com/sdk-documentation`
- Requisitos: `https://sdk.poki.com/new-requirements.html`
- P4D: `https://sdk.poki.com/what-is-p4d.html`
- Inspector: `https://sdk.poki.com/poki-inspector.html`
- Final Review: `https://sdk.poki.com/final-review.html`
- Deals: `https://sdk.poki.com/deals`
- FAQ: `https://sdk.poki.com/faq`
