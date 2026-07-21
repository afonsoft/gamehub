# 10 - Checklist de Validação

## Repositório

- [ ] Branch dedicada criada.
- [ ] Nenhum arquivo sensível commitado.
- [ ] `.env.example` criado.
- [ ] `docs/agent-execution-log.md` criado.

## Backend

- [ ] Solution identificada ou criada.
- [ ] Projetos Core/Application/EF/Web.Host localizados.
- [ ] Entidades principais criadas (ver `04-modelagem-dados.md`).
- [ ] DbSets configurados.
- [ ] Fluent API por entidade.
- [ ] Application Services criados (ver `09-prompt-agent-cli.md` Etapa 4).
- [ ] DTOs criados (ver `05-api-contratos.md`).
- [ ] Permissões administrativas definidas.
- [ ] Upload endpoint criado.
- [ ] Validador de build criado.
- [ ] Redis abstractions criadas.
- [ ] Logs estruturados configurados.
- [ ] Health checks configurados.
- [ ] Permissions ABP configurados (ver `12-rbac-permissions.md`).
- [ ] Health check endpoint funcional.
- [ ] Serilog JSON output configurado.
- [ ] OpenTelemetry traces exportando.

## Frontend Game Hub

- [ ] Módulos Public/Player/Developer criados.
- [ ] Home pública criada.
- [ ] Página de detalhe do jogo criada.
- [ ] GameShell com iframe sandbox criado.
- [ ] Developer upload wizard criado.
- [ ] Serviços HTTP criados.
- [ ] Design system próprio implementado.
- [ ] Rotas configuradas com lazy loading (ver `13-frontend-routing.md`).
- [ ] Guards funcionando (auth, developer, guest).
- [ ] Interceptors configurados (JWT, error, correlationId).
- [ ] GameplayBridge completo (10 eventos).
- [ ] Design tokens aplicados.

## Frontend Admin

- [ ] Módulos Games/Moderation/Categories/Tags/Dashboard criados.
- [ ] Tabela de jogos com filtros criada.
- [ ] Fila de moderação criada.
- [ ] CRUD categorias/tags criado.
- [ ] Dashboard de métricas criado.
- [ ] Serviços HTTP criados.
- [ ] Rotas configuradas com lazy loading (ver `13-frontend-routing.md`).
- [ ] Guards funcionando (auth, admin, moderator).
- [ ] Tabelas com paginação e filtros.
- [ ] Confirmação em ações destrutivas.

## DevOps

- [ ] Docker Compose possui backend, hub, admin, DB e Redis.
- [ ] Dockerfiles do template EAF verificados e funcionando.
- [ ] Scripts idempotentes criados.
- [ ] Migração/seed documentados.
- [ ] CI/CD sugerido documentado.
- [ ] DNS configurado (gamehub.afonsoft.dev, gamehub-admin.afonsoft.dev, gamehub-api.afonsoft.dev).

## Segurança

- [ ] Iframe sandbox aplicado.
- [ ] CSP documentada/configurada.
- [ ] CORS configurado para dois frontends.
- [ ] Rate limiting previsto.
- [ ] Workflow de moderação criado.
- [ ] LGPD documentada.
- [ ] CSP header verificado (ver `15-csp-security-headers.md`).
- [ ] CORS preflight testado entre dois frontend.
- [ ] JWT token expira corretamente.
- [ ] Rate limiting respondendo com headers `X-RateLimit-*`.
- [ ] HTTPS configurado em produção.

## Testes

- [ ] Testes de domínio.
- [ ] Testes de application service.
- [ ] Testes de build validator.
- [ ] Testes de leaderboard cache/stub.

## Qualidade

- [ ] `dotnet build` executado.
- [ ] `dotnet test` executado.
- [ ] `npm run build` (hub) executado.
- [ ] `npm run build` (admin) executado.
- [ ] `docker compose config` executado.
- [ ] Falhas registradas sem ocultação.
- [ ] Home carrega em <2s (performance).
- [ ] Busca retorna em <500ms (performance).
- [ ] i18n funciona (troca pt-BR/en-US).
- [ ] Navegação por teclado funciona (acessibilidade).
- [ ] Screen reader consegue navegar (acessibilidade).
- [ ] Testes de segurança executados (ver `15-csp-security-headers.md`).
