# Módulos do ASP.NET Boilerplate (Base)

O EAF é construído sobre o ASP.NET Boilerplate (ABP). Esta seção fornece uma visão geral dos módulos do ABP que são usados pelo EAF.

## Módulos do ABP

O ABP fornece um conjunto de módulos que formam a base do framework:

### Core Modules

- **AbpZeroModule**: Módulo base do ABP Zero, fornecendo funcionalidades de multi-tenancy, autenticação, autorização, etc.
- **AbpKernelModule**: Módulo kernel do ABP, fornecendo funcionalidades core como injeção de dependência, configuração, etc.

### Authentication & Authorization

- **Autenticação**: Sistema de autenticação com suporte a cookies, JWT e provedores externos
- **Autorização**: Sistema de autorização baseado em permissões e roles
- **Multi-tenancy**: Suporte a multi-tenancy com isolamento de dados por tenant

### Data Access

- **Entity Framework Core**: Integração com Entity Framework Core para acesso a dados
- **Repositories**: Padrão Repository implementado com genéricos e customizados
- **Unit of Work**: Padrão Unit of Work para gerenciamento de transações

### Background Jobs

- **Hangfire**: Integração com Hangfire para background jobs
- **Background Job Manager**: Sistema de gerenciamento de background jobs do ABP

### Logging & Caching

- **Logging**: Sistema de logging integrado com Castle Windsor
- **Caching**: Sistema de caching com suporte a Redis, memória e outros backends

### Notifications

- **Notification System**: Sistema de notificações do ABP
- **Real-time Notifications**: Suporte a notificações em tempo real via SignalR

## Documentação Detalhada

Para documentação detalhada dos módulos do ABP, consulte:

- [Documentação Oficial do ASP.NET Boilerplate](https://aspnetboilerplate.com/Pages/Documents)
- [Módulos do ABP](https://aspnetboilerplate.com/Pages/Documents/Module-System)

## Integração com EAF

O EAF estende os módulos do ABP com funcionalidades específicas:

- **Eaf.Castle.Serilog**: Logging estruturado com Serilog
- **Eaf.KeyVault**: Gerenciamento de segredos com Azure Key Vault
- **Eaf.Middleware.AzureActiveDirectory**: Autenticação com Azure AD
- **Eaf.Middleware.Ldap**: Autenticação com LDAP/Active Directory
- **Eaf.OpenTelemetry**: Observabilidade com OpenTelemetry
- **Eaf.SqlServerCache / Eaf.SqliteCache**: Cache distribuído com SQL Server/SQLite
