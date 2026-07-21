# 02 - Funcionalidades do Produto

## Personas

### Jogador anônimo

- Acessa home pública.
- Pesquisa jogos.
- Filtra por categoria, tag, dispositivo e popularidade.
- Abre página de detalhe do jogo.
- Joga em iframe/sandbox.
- Vota/curte/descurte.
- Reporta problema ou conteúdo inadequado.

### Jogador autenticado

- Mantém perfil público simples.
- Salva jogos favoritos.
- Continua progresso quando o jogo suportar save cloud.
- Participa de leaderboards.
- Recebe recomendações personalizadas.

### Desenvolvedor/Publicador

- Cria perfil de desenvolvedor.
- Submete jogo para revisão.
- Faz upload de builds versionadas.
- Preenche metadados, screenshots, thumbnail, categorias, idioma e classificação indicativa.
- Visualiza status de revisão.
- Consulta métricas básicas de performance e gameplay.

### Moderador

- Revisa jogos submetidos.
- Aprova, reprova ou solicita ajustes.
- Analisa reports dos usuários.
- Bloqueia builds problemáticas.

### Administrador

- Gerencia usuários, roles e permissões.
- Configura categorias, tags e curadoria da home.
- Controla feature flags.
- Acompanha métricas e auditoria.

## Funcionalidades públicas

- Home com seções:
  - Destaques
  - Novos jogos
  - Mais jogados
  - Tendências
  - Recomendados
  - Categorias
- Página de jogo:
  - Título, descrição, instruções, controles, tags.
  - Botão jogar.
  - Iframe sandbox.
  - Jogos relacionados.
  - Votação e favoritos.
- Busca:
  - Texto livre (full-text search via PostgreSQL).
  - Categorias.
  - Tags.
  - Dispositivo suportado.
  - Orientação: landscape/portrait/both.

### Critérios de aceite

- Home carrega em <2s com cache ativo.
- Cards mostram thumbnail, título, categorias e suporte a dispositivos.
- Busca retorna resultados em <500ms.
- Filtros persistem na URL e são compartilháveis.
- Página de detalhe mostra todos os metadados do jogo.

## Funcionalidades de gameplay/eventos

Inspirado nos eventos documentados pelo SDK Poki, criar eventos internos equivalentes:

| Evento | Descrição |
|--------|-----------|
| `GameLoadingStarted` | Início do carregamento do jogo |
| `GameLoadingFinished` | Carregamento concluído |
| `GameplayStarted` | Jogador iniciou a sessão |
| `GameplayStopped` | Jogador pausou/saiu |
| `CommercialBreakRequested` | Jogo solicita break comercial |
| `CommercialBreakCompleted` | Break comercial finalizado |
| `RewardedBreakRequested` | Jogo solicita rewarded ad |
| `RewardedBreakCompleted` | Rewarded ad finalizado |
| `GameErrorCaptured` | Erro capturado no jogo |
| `GameMeasuredEvent` | Evento de medição customizado |

Esses eventos devem ser enviados pelo wrapper JavaScript da plataforma e persistidos de forma agregada, evitando gravar excesso de eventos brutos sem necessidade.

### Critérios de aceite

- Eventos são enviados em tempo real via `postMessage`.
- Backend persiste eventos com timestamp do servidor.
- Sessão é encerrada automaticamente se não houver heartbeat por 30s.

## Funcionalidades administrativas (app separada)

- Gestão de jogos.
- Gestão de builds.
- Gestão de categorias/tags.
- Workflow de revisão.
- Auditoria de mudanças.
- Feature flags.
- Dashboard de métricas.
- Gestão de reports.

## Funcionalidades para upload de jogos

- Upload zip/tar de build HTML5/WebGL.
- Validação obrigatória:
  - index.html presente.
  - tamanho máximo configurável (ex: 100MB).
  - extensão de arquivos permitida.
  - ausência de arquivos executáveis nativos (.exe, .dll, .bat, .cmd, .ps1).
  - ausência de scripts externos não permitidos.
  - manifest.json opcional com metadados.
- Geração de versão imutável (semver: 1.0.0).
- Hash SHA-256 do pacote.
- Publicação em storage público/CDN apenas após aprovação.

### Critérios de aceite

- Upload aceita apenas `.zip` e `.tar.gz`.
- Validação roda em <30s para pacotes de até 100MB.
- Relatório de validação mostra erros específicos com caminho do arquivo.
- Build aprovado gera URL pública no CDN.
- Build rejeitado mantém o histórico para consulta.

## Monetização

- Preparar interfaces para:
  - Ads comerciais em pausas naturais.
  - Rewarded ads.
  - Revenue share por desenvolvedor.
- Implementação inicial pode usar provider fake/null object.
- Não acoplar domínio ao fornecedor de ads.

### Critérios de aceite

- Recomendações personalizadas e save na nuvem estão fora do escopo MVP.
- Interface `IAdProvider` definida com provider fake/null object funcional.
- Domain não referencia implementações concretas de ads.

## Localização (i18n)

- Plataforma em pt-BR e en-US no MVP.
- Usar `IStringLocalizer` do ABP para backend.
- Usar `@angular/localize` ou similar para frontend.
- Jogos podem declarar idiomas suportados.
- Metadados localizáveis:
  - título
  - descrição
  - instruções
  - changelog
  - tags customizadas

### Critérios de aceite

- _texts do backend usam `IStringLocalizer` com arquivos `.json` por cultura.
- Frontend usa `@angular/localize` com arquivos `.xlf`.
- pt-BR e en-US disponíveis no MVP.
- Strings não são hardcoded em componentes ou services.
