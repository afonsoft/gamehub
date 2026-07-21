# 00 - Contexto e Fontes Consultadas

## Objetivo

Criar uma plataforma de jogos web inspirada em portais de catálogo e distribuição de games web, com foco em:

- Catálogo público de jogos
- Experiência responsiva desktop/mobile/tablet
- Execução segura de jogos HTML5/WebGL por embed/sandbox
- Portal administrativo (app separada)
- Portal para desenvolvedores/publicadores
- Métricas de gameplay, qualidade, engajamento e monetização
- Backend API com .NET 10 LTS, template legado AspZero/EAF, EF Core e Redis
- Frontends Angular 20+ separados: game hub + admin

## Ambiente de produção

| Serviço | DNS | Stack |
|---------|-----|-------|
| API | `gamehub-api.afonsoft.dev` | .NET 10 LTS (Docker) |
| Game Hub | `gamehub.afonsoft.dev` | Angular 20+ (Docker) |
| Admin | `gamehub-admin.afonsoft.dev` | Angular 20+ (Docker) |

**Infraestrutura do servidor**: PostgreSQL e Redis já gerenciados no servidor. O template EAF já possui Dockerfiles para API e admin.

## Arquitetura de frontends

A plataforma possui **duas aplicações Angular independentes** consumindo a mesma API backend:

| App | Diretório | DNS (prod) | Escopo | Porta (dev) |
|-----|-----------|------------|--------|-------------|
| Game Hub | `angular/` | `gamehub.afonsoft.dev` | Catálogo, jogos, busca, perfil dev | 4200 |
| Admin | `angular-admin/` | `gamehub-admin.afonsoft.dev` | Gestão, moderação, métricas, config | 4201 |
| API | `aspnet-core/` | `gamehub-api.afonsoft.dev` | Backend REST | 5000 |

## Referências públicas analisadas

### Poki

- Poki for Developers: https://developers.poki.com/
- Poki Documentation: https://sdk.poki.com/
- SDK Documentation: https://sdk.poki.com/sdk-documentation
- Requirements: https://sdk.poki.com/new-requirements
- Quality Guidelines: https://sdk.poki.com/poki-quality-guidelines
- Localization: https://sdk.poki.com/localization
- Poki CLI: https://github.com/poki/poki-cli
- Poki npm-sdk: https://github.com/poki/npm-sdk/

Pontos extraídos das documentações públicas:

- A experiência deve funcionar bem em desktop, mobile e tablet.
- Jogos web precisam ser otimizados para diferentes dispositivos, conexões e navegadores.
- É importante ter suporte a ciclo de vida de gameplay: carregamento finalizado, início de gameplay, parada/pausa, breaks comerciais e rewarded breaks.
- Para distribuição web, qualidade, originalidade, UX responsiva, acessibilidade e conteúdo seguro para todas as idades são critérios relevantes.
- Localização deve ser planejada desde o início, centralizando textos em arquivos estruturados.
- Upload/versionamento de builds pode ser automatizado por CLI em pipelines.

### ASP.NET Boilerplate (legado)

- Documentação: https://aspnetboilerplate.com/Pages/Documents
- Startup Template Angular: https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular
- GitHub: https://github.com/aspnetboilerplate/aspnetboilerplate

Pontos extraídos:

- Framework open source com modelo arquitetural baseado em DDD.
- Integra DI, Repositories, Authorization, Validation, Audit Logging, Unit of Work, Exception Handling, Logging, Localization, Auto Mapping e Dynamic API Layer.
- Template Angular usa ASP.NET Core backend e frontend Angular separado.
- Template possui autenticação JWT, multi-tenancy por padrão, migrator console, testes e suporte Docker.

> **Nota**: O ABP legado (aspnetboilerplate) está em modo de manutenção. O template utilizado é o EAF, que é baseado no legado AspZero/EAF.

### EAF (template base)

- Repositório: https://github.com/afonsoft/EAF
- Templates: https://github.com/afonsoft/EAF/tree/main/Templates

Pontos extraídos:

- Enterprise Application Foundation baseado no template legado AspZero/EAF.
- Foco em ASP.NET Core/EF Core moderno.
- Substitui/expande logging com Serilog.
- Inclui OpenTelemetry, Azure/Oracle KeyVault, Hangfire e middlewares próprios.
- Angular é usado como frontend na base atual.

## Decisões propostas

- Usar o template legado AspZero/EAF como base (https://github.com/afonsoft/EAF/tree/main/Templates).
- .NET 10 LTS como baseline.
- Angular 20+ com design system próprio (não Angular Material).
- Dois frontends Angular separados: game hub e admin.
- Manter Clean Architecture e DDD com separação clara entre Core, Application, EntityFrameworkCore, Web.Host e Angular.
- Implementar Redis como cache, rate limiting e suporte a ranking/leaderboards.
- Começar com monólito modular bem delimitado, preparado para extração futura de serviços.
- Não usar FluentValidation (validação nativa do ABP) nem MediatR (CQRS nativo do ABP).
- Garantir que a plataforma não copie assets, identidade visual ou jogos de terceiros.

## Padrões adotados do Poki

Após análise das documentações públicas do Poki, adotamos os seguintes padrões como base para a plataforma GameHub:

- **Gameplay lifecycle events**: ciclo de vida-padronizado de gameplay cobrindo `loading`, `start`, `stop`, `commercial break` e `rewarded break`. Esses eventos são expostos via `GameplayBridgeService` e persistidos pelo backend de forma agregada.
- **Responsive UX**: a experiência deve funcionar bem em desktop, mobile e tablet, com layout adaptativo e suporte a orientação landscape/portrait.
- **Quality guidelines**: critérios mínimos de qualidade para o catálogo de jogos (performance, originalidade, acessibilidade, conteúdo seguro para todas as idades). Aplicados no fluxo de moderação.
- **Localization from the start**: toda string de UI é centralizada desde o início em arquivos estruturados, prontos para `pt-BR` e `en-US`.

## Padrões rejeitados/modificados do Poki

Alguns padrões do Poki foram intencionalmente rejeitados ou adaptados para a realidade da GameHub:

- **Poki SDK coupling** → recusado. A plataforma não acopla jogos ao SDK proprietário do Poki. Em vez disso, usa sua abstração própria `GameplayBridgeService` que expõe o mesmo conjunto de eventos sem amarrar o jogo a um fornecedor específico.
- **Poki's proprietary ad system** → recusado. A plataforma não integra o sistema de anúncios fechado do Poki. Em vez disso, expõe a interface abstrata `IAdProvider` com um provider fake/null object no MVP, permitting trocar o provedor de ads no futuro sem alterar o domínio.
- **Poki's CLI for uploads** → recusado. Substituído por upload web com validação server-side completa (tamanho, arquivos permitidos, presença de `index.html`, ausência de executáveis, geração de SHA-256). O fluxo é totalmente auditável pelo portal administrativo.

## Fora do escopo no MVP

As funcionalidades abaixo estão deliberadamente fora do escopo do MVP e não devem ser implementadas nesta fase:

- **Save na nuvem (cloud save)**: persistência remota de progresso dos jogos (mock apenas, interface preparada para o futuro).
- **Recomendações personalizadas**: algoritmo de recomendação personalizado (apenas trending/destaques simples no MVP).
- **Monetização real**: apenas interface preparada com provider fake (`IAdProvider`). Não haverá integração real com networks de anúncios no MVP.
- **Multi-tenancy**: plataforma single-tenant no MVP (suporte do ABP legado disponível, mas não habilitado).
- **Mobile nativo**: apenas PWA opcional; sem apps nativos iOS/Android.
- **Chat/social features**: comunicación entre usuários, chat, comentários, fóruns.
- **Pagamentos/payments**: integração com gateways de pagamento, split de receita automatizado. A Fase 7 (Monetização) é pós-lançamento.
