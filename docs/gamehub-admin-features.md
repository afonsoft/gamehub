# GameHub Admin — Funcionalidades / Features

## Português (pt-BR)

O **GameHub Admin** é o painel de gestão da plataforma, com controle de jogos, empresas, builds, moderação, relatórios e configurações.

### Dashboard
- Visão geral por perfil: admin vê métricas da plataforma, moderador vê fila de revisão e desenvolvedor vê seus jogos, builds e ações pendentes.
- Exibe o tenant atual e gráfico de plays ao longo do tempo.

### Games
- Listagem com filtros por status (Draft, Submitted, InReview, ApprovedForPublishing, Published, Rejected, Suspended, Archived).
- Ações do ciclo de vida: iniciar revisão, aprovar para publicação, solicitar alterações, publicar (com build ID), suspender.
- Tabela padrão com paginação, lazy load e status badges.

### Uploads / Builds
- Lista de builds por status: Uploaded, Validating, Validated, ValidationFailed, InReview, Approved, Published, Rejected, Blocked.
- Visualização de arquivos e link para o jogo.

### Inspector
- Sessões de inspeção para validar integração do SDK e checklist.

### Moderação
- Fila de revisões filtrada por status (Pending, InProgress, Completed).
- Tela de detalhe com decisão (Approved, Rejected, RequiresChanges) e notas.

### Reports
- Denúncias de usuários com atualização de status (Open, UnderReview, Resolved, Dismissed).

### Companies
- Cadastro de empresas (tenants) com tenancy name, nome, e-mail e país.
- Gerenciamento de funcionários: convite, definição de padrão, remoção e controle de permissões.

### Test Session & API Sandbox
- Testar jogos antes da publicação via token de preview/playtest.
- Explorar a API via Swagger UI (quando disponível) ou exemplos curl offline.

### Configurações
- Feature Flags, Audit Log e controle de usuários.

## English (en-US)

**GameHub Admin** is the platform management panel, controlling games, companies, builds, moderation, reports and settings.

### Dashboard
- Profile-aware overview: admins see platform metrics, moderators see review queue and developers see their games, builds and pending actions.
- Shows current tenant and plays-over-time chart.

### Games
- Listing with status filters (Draft, Submitted, InReview, ApprovedForPublishing, Published, Rejected, Suspended, Archived).
- Lifecycle actions: start review, approve for publishing, request changes, publish (with build ID), suspend.
- Standard table with pagination, lazy loading and status badges.

### Uploads / Builds
- Build list by status: Uploaded, Validating, Validated, ValidationFailed, InReview, Approved, Published, Rejected, Blocked.
- File view and link to game.

### Inspector
- Inspection sessions to validate SDK integration and checklist.

### Moderation
- Review queue filtered by status (Pending, InProgress, Completed).
- Detail screen with decision (Approved, Rejected, RequiresChanges) and notes.

### Reports
- User reports with status update (Open, UnderReview, Resolved, Dismissed).

### Companies
- Company (tenant) registration with tenancy name, name, email and country.
- Employee management: invite, set default, remove and permission control.

### Test Session & API Sandbox
- Test games before publishing via preview/playtest token.
- Explore the API via Swagger UI (when available) or offline curl examples.

### Settings
- Feature Flags, Audit Log and user management.
