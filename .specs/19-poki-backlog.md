# 19 - Backlog Inspirado na Poki

> **Status:** Em evolução  
> **Base:** `.specs/18-poki-referencia.md` e análise do site/documentação Poki  
> **Objetivo:** Direcionar próximas fases do GameHub com conceitos comprovados em uma plataforma de jogos web, sem copiar marca, layout ou conteúdo.
> **Última re-análise:** 2026-07-24

---

## 1. Visão

O GameHub já possui o fluxo beta funcional: catálogo, player, developer portal, moderação e admin. O próximo passo é tornar a experiência pública mais atrativa e o processo de publicação mais parecido com o padrão de qualidade do mercado, usando a Poki como referência.

---

## 2. Fase 1 - Polimento do portal público

### 2.1 Página de jogo

- Like / Dislike por jogo (com contador visível).
- Botão "Report a bug" para reportes técnicos (diferente de "Report game" para conteúdo).
- Botão de tela cheia (fullscreen) no player.
- Seção de controles: listar teclas desktop e gestos mobile.
- Descrição expandida com "Show more".
- Jogos relacionados e da mesma categoria abaixo do jogo.
- Categorias/tags do jogo clicáveis, levando ao catálogo filtrado.

### 2.2 Home e descoberta

- Seção "Popular this week".
- Seção "Top free games / Trending now" baseada em plays reais.
- Seção "Web exclusives" para jogos com contrato de exclusividade.
- Cards com nota média (estrelas) e número de votos.
- SEO das páginas de categoria.

### 2.3 Contas opcionais de jogador

- Login via Apple/Google/Microsoft/passkey (quando configurado).
- Salvamento de favoritos.
- Histórico de jogos jogados recentemente.
- Progresso sincronizado entre dispositivos.

---

## 3. Fase 2 - Melhorias no portal do desenvolvedor

### 3.1 Submissão de jogo

- Wizard de submissão com etapas claras: metadados, thumbnail/hero, build, validação, categorias sugeridas e descrição SEO.
- Sistema de anotações/notes para o moderador.
- Pré-visualização de como o jogo ficará na página pública.

### 3.2 Categorias e descrição sugeridas

- Desenvolvedor pode sugerir até 4 categorias.
- Campo de descrição sugerida com guias de SEO.
- Moderador pode aprovar ou ajustar as sugestões na revisão.

### 3.3 Revisão de thumbnails

- Após o jogo estar publicado, qualquer atualização de thumbnail/hero entra em fila de moderação.
- Notificação ao desenvolvedor quando aprovada/rejeitada.

### 3.4 Métricas do desenvolvedor

- Total de plays.
- Daily Active Users (jogando vs não jogando).
- Engagement (tempo médio por DAU).
- Erros e eventos de gameplay.
- Filtro por país e dispositivo.

---

## 4. Fase 3 - SDK e cloud saves

### 4.1 Alinhamento do SDK com Poki

- `GameHubSDK.init()` retornando Promise.
- `gameLoadingFinished()` após carga completa.
- `gameplayStart()` no primeiro input do jogador.
- `gameplayStop()` em pausa, game over, menu, cutscene.
- `commercialBreak()` em transições naturais.
- `rewardedBreak()` com opções de `size` e `onStart`.
- Regras de sequência: não permitir dois eventos iguais seguidos.

### 4.2 Incognito e storage

- Todos os acessos a `localStorage` e `IndexedDB` devem estar em try/catch.
- Prefixo `gamehub_ignore` para chaves que não devem ser sincronizadas.

### 4.3 Cloud saves

- Sincronização automática de `localStorage` e `IndexedDB` para usuários logados.
- Limite de 1 MB após gzip.
- Fallback para storage local quando offline ou não logado.

### 4.4 Login do jogador no SDK

- `GameHubSDK.login()` abre painel OAuth.
- `GameHubSDK.getUser()` retorna `{ username, avatarUrl }`.
- `GameHubSDK.getToken()` retorna JWT de curta duração para validação no backend do jogo.

---

## 5. Fase 4 - Monetização

### 5.1 Ad provider

- Interface `IAdProvider` para breaks comerciais e recompensados.
- Integração com provedor externo ou simulador em dev.

### 5.2 Regras de ads

- Mudo automático de áudio durante anúncios.
- Não implementar timers internos de anúncios.
- Um rewarded break por recompensa.
- Botão padrão sempre disponível e maior que o botão de recompensa.
- Botões de recompensa com ícones de vídeo e cor distinta.
- Não entregar recompensa quando ad block é detectado.
- Não exibir mensagens customizadas de ad block.

### 5.3 Modelo de receita

- Cadastro de contrato por jogo: web exclusive ou non-exclusive.
- Tracking de origem do jogador (utm, referrer, busca, homepage).
- Divisão: 100% para dev quando o jogador vem direto, 50/50 quando a plataforma traz.
- Painel de earnings e ad performance no portal do desenvolvedor.

---

## 6. Fase 5 - Qualidade e Inspector

### 6.1 Requisitos de build

- Tamanho inicial abaixo de 8 MB (recomendado).
- Proporção 16:9 (referências: 640x360, 836x470, 1031x580).
- Suporte a 30 FPS mínimo, 60 FPS alvo.
- Remover debug code, artifacts de teste e ferramentas de dev do pacote.

### 6.2 Validações adicionais

- Checagem de requests externos.
- Otimização de imagens (compressão e formato).
- Aviso de arquivos grandes ou desnecessários.
- Teste de incognito.

### 6.3 GameHub Inspector

- Página interna para upload de build e teste.
- Log de eventos do SDK em tempo real.
- Checklist de QA por módulos.
- Seleção de dimensões e dispositivos para scaling tests.
- Aba de warnings (recursos externos, imagens, SDK).

---

## 7. Fase 6 - Performance e observabilidade

### 7.1 Métricas de player

- Play count, DAU, MAU.
- Taxa de carregamento completo (conversion to play).
- Tempo médio de sessão.
- Eventos de gameplay e erros.
- Distribuição por país, dispositivo e navegador.

### 7.2 Alertas

- Jogos com taxa de erro acima do limiar.
- Jogos com loading alto.
- Jogos com baixo engajamento.

---

## 8. Estado por spec filho

| Spec | Foco | Status |
|------|------|--------|
| `19.1` | Página de jogo (controls, related, rating) | Parcialmente implementado (PR #42) |
| `19.2` | Home e descoberta | Parcialmente implementado (home com seções) |
| `19.3` | SDK, cloud saves e incognito | Base implementada |
| `19.4` | Portal do desenvolvedor (SEO/campos) | Implementado (PR #44) |
| `19.5` | Inspector e relatório de validação | Base implementada (PR #45) |
| `19.6` | Monetização e earnings | Implementado (PR #46 e #47) |
| `19.7` | Métricas e alertas admin | Implementado (PR #44) |
| `19.8` | Contas de jogador e favoritos | Implementado |
| `19.9` | Integração do Ad Provider | Implementado |
| `19.10` | Inspector de QA v2 | Planejado — criar prompt para sessão dedicada |
| `19.11` | Web exclusives e descoberta avançada | Implementado |
| `19.12` | Privacidade, UGC e performance | Planejado — criar prompt para sessão dedicada |

## 9. Ordem sugerida de implementação (pós re-análise)

1. `19.11` - Web exclusives e descoberta (baixo risco, destaca contratos existentes).
2. `19.8` - Contas de jogador e favoritos (aumenta retenção).
3. `19.10` - Inspector de QA v2 (melhora qualidade antes de escalar).
4. `19.9` - Integração do Ad Provider (habilita receita real).
5. `19.12` - Privacidade, UGC e performance (conformidade e saúde da plataforma).

---

## 10. Critérios de aceite gerais

- Cada feature deve ter testes backend e/ou frontend.
- Build e testes devem passar.
- Nenhum secret ou configuração real commitada.
- i18n em pt-BR e en-US.
- Documentação em `docs/` e atualização do `CHANGELOG.md`.

---

## 11. Links úteis

- `.specs/18-poki-referencia.md`
- `https://sdk.poki.com/html5`
- `https://sdk.poki.com/new-requirements.html`
- `https://sdk.poki.com/what-is-p4d.html`
- `https://sdk.poki.com/poki-inspector.html`
