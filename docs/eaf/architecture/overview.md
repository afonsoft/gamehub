# Visão Geral da Arquitetura EAF

Bem-vindo à visão geral da arquitetura do Sistema EAF (Enterprise Application Framework). Este documento tem como objetivo fornecer uma compreensão de altoível dos principais componentes arquiteturais, princípios de design e como eles interagem para formar a espinha dorsal do sistema.

O EAF é uma plataforma robusta e extensível projetada para acelerar o desenvolvimento de aplicações corporativas modernas. Ele é construído sobre o [ASP.NET Boilerplate (ABP)](https://aspnetboilerplate.com/), um framework de aplicação de código aberto bem estabelecido, e aproveita sua arquitetura N-Layer (N-Camadas) para promover uma clara separação de preocupações e modularidade.

A arquitetura N-Layer do ABP, adotada pelo EAF, consiste principalmente nas seguintes camadas:

1.  **Camada de Domínio (Domain Layer)**: Esta é o coração da aplicação e contém a lógica de negócios central. Inclui:
    *   **Entidades (Entities)**: Objetos de domínio que representam dados e comportamento.
    *   **Serviços de Domínio (Domain Services)**: Lógica de negócios que não pertence naturalmente a uma única entidade.
    *   **Repositórios (Repositories)**: Abstrações para acesso a dados, definindo interfaces para consultar e persistir entidades.
    *   **Objetos de Valor (Value Objects)**, **Eventos de Domínio (Domain Events)**, etc.
    No EAF, esta camada encapsula as regras de negócios específicas da aplicação que está sendo construída.

2.  **Camada de Aplicação (Application Layer)**: Esta camada orquestra as interações entre a camada de apresentação e a camada de domínio. Ela contém:
    *   **Serviços de Aplicação (Application Services)**: Expõem a funcionalidade de negócios para a camada de apresentação. Eles utilizam os serviços de domínio e repositórios para realizar operações, aplicam autorização e podem implementar a lógica de transação.
    *   **Objetos de Transferência de Dados (Data Transfer Objects - DTOs)**: Usados para passar dados entre a camada de apresentação e os serviços de aplicação.
    No EAF, os serviços de aplicação são os principais pontos de entrada para as funcionalidades do sistema a partir da UI ou APIs externas.

3.  **Camada de Infraestrutura (Infrastructure Layer)**: Esta camada lida com preocupações técnicas transversais e implementa abstrações definidas nas camadas superiores (especialmente interfaces de repositório). Inclui:
    *   **Persistência de Dados**: Implementações de repositórios usando tecnologias como Entity Framework Core.
    *   **Integrações**: Conexão com sistemas externos, como serviços de mensageria, caches distribuídos, etc.
    *   **Logging, Auditoria**: Implementações concretas para essas preocupações.
    O EAF utiliza esta camada para integrar-se com bancos de dados (SQL Server, Oracle, etc.), provedores de cache (Redis), e outros serviços de infraestrutura.

4.  **Camada de Apresentação (Presentation Layer)**: Esta é a interface com o usuário ou sistemas externos. Pode ser uma aplicação Web (ASP.NET Core MVC, Angular, React), uma API RESTful, ou outras formas de interação. O EAF normalmente utiliza ASP.NET Core MVC para a interface do usuário administrativa e APIs RESTful para integrações e front-ends modernos.

## Principais Preocupações Transversais (Cross-Cutting Concerns)

O EAF herda e estende muitas das robustas preocupações transversais fornecidas pelo ASP.NET Boilerplate, incluindo:

*   **Injeção de Dependência (Dependency Injection)**: Utilizada extensivamente para gerenciar dependências entre classes.
*   **Logging**: Capacidades de log para registrar eventos e erros do sistema.
*   **Caching**: Suporte para caching de dados para melhorar o desempenho.
*   **Multi-tenancy**: Suporte para aplicações que atendem a múltiplos clientes (tenants) de forma isolada.
*   **Background Jobs**: Gerenciamento e execução de tarefas em segundo plano.
*   **Auditoria (Auditing)**: Rastreamento automático de alterações em entidades e interações do usuário.
*   **Validação (Validation)**: Validação de DTOs e entradas de métodos.
*   **Autorização (Authorization)**: Controle de acesso baseado em permissões.
*   **Unit of Work (UOW)**: Gerenciamento de transações de banco de dados.

## Componentes e Extensões Específicas do EAF

Além das funcionalidades do ABP, o EAF introduz componentes e extensões arquiteturais específicos para atender a requisitos corporativos avançados. Alguns exemplos notáveis incluem:

*   **Integração com Azure Key Vault**: Para gerenciamento seguro de segredos e configurações.
*   **Manipulação Específica de Azure AD / LDAP**: Mecanismos aprimorados ou mais flexíveis para autenticação e sincronização de usuários com Azure Active Directory e LDAP.
*   **Observabilidade com OpenTelemetry**: Integração com OpenTelemetry para coleta de métricas, traces e logs, permitindo um monitoramento mais profundo da aplicação.
*   **Módulos de Negócio Reutilizáveis**: Um conjunto de módulos de negócio pré-construídos que podem ser facilmente integrados em novas aplicações.

## Diagrama da Arquitetura EAF

A seguir, descreve-se o diagrama arquitetural principal do EAF, que ilustra a estrutura em N-Camadas e a integração dos componentes específicos do EAF. Este diagrama visa fornecer uma representação visual clara das principais partes do sistema e suas interações.

**Título do Diagrama:** Arquitetura N-Camadas do EAF com Extensões Chave

**Representação Visual:**

O diagrama deve ser organizado em quatro camadas horizontais principais, com preocupações transversais listadas verticalmente ao lado delas.

1.  **Camada de Apresentação (Presentation Layer - Topo):**
    *   **Componentes Principais:**
        *   "ASP.NET Core MVC (Admin UI)"
        *   "RESTful APIs (para SPAs, Mobile, Integrações)"
        *   "Aplicações Cliente (Ex: Angular, React, Mobile)" - Representadas como consumidoras externas das APIs.
    *   **Fluxo Principal:** Interage com a Camada de Aplicação.

2.  **Camada de Aplicação (Application Layer - Segunda Camada):**
    *   **Componentes Principais:**
        *   "Serviços de Aplicação (Application Services)"
        *   "Objetos de Transferência de Dados (DTOs)"
    *   **Extensões EAF Relevantes:**
        *   "Configuração de Autenticação Externa por Tenant" (orquestra a lógica de autenticação que pode ser configurada no nível do tenant).
    *   **Fluxo Principal:** Utiliza serviços e entidades da Camada de Domínio; pode interagir com a Camada de Infraestrutura para preocupações como Unit of Work.

3.  **Camada de Domínio (Domain Layer - Terceira Camada):**
    *   **Componentes Principais:**
        *   "Entidades (Entities)"
        *   "Serviços de Domínio (Domain Services)"
        *   "Interfaces de Repositório (Repository Interfaces)"
        *   "Objetos de Valor (Value Objects)"
        *   "Eventos de Domínio (Domain Events)"
    *   **Fluxo Principal:** Contém a lógica de negócios central. Suas interfaces (como repositórios) são implementadas pela Camada de Infraestrutura.

4.  **Camada de Infraestrutura (Infrastructure Layer - Camada Inferior):**
    *   **Componentes Principais:**
        *   "Persistência de Dados (EF Core - SQL Server, Oracle, etc.)"
        *   "Implementações de Repositório"
        *   "Serviços de Integração (Mensageria, etc.)"
    *   **Extensões EAF Chave (dentro ou gerenciadas por esta camada):**
        *   "Integração com Azure Key Vault (`Eaf.KeyVault`)" (gerenciamento de segredos)
        *   "Integração com Azure AD (`Eaf.Middleware.AzureActiveDirectoryModule`)" (autenticação)
        *   "Integração com LDAP (`Eaf.Middleware.LdapModule`)" (autenticação)
        *   "Logging Customizado (Serilog, Appender para Azure Service Bus)"
        *   "Soluções de Cache EAF (SQL Server Cache, SQLite Cache)"
        *   "Observabilidade com OpenTelemetry (`Eaf.OpenTelemetry`)"
        *   "Infraestrutura de Background Jobs (Hangfire)" (com workers EAF como `ExpiredAuditLogDeleterWorker` operando sobre ela).
    *   **Fluxo Principal:** Implementa interfaces das camadas superiores (Domínio, Aplicação). Interage com sistemas externos como bancos de dados, Azure Key Vault, Azure AD, LDAP, sistemas de mensageria e plataformas de observabilidade.

**Preocupações Transversais (Cross-Cutting Concerns):**

*   Representadas por uma barra vertical ou anotações ao lado de todas as camadas:
    *   "Injeção de Dependência"
    *   "Logging"
    *   "Caching"
    *   "Multi-tenancy"
    *   "Background Jobs"
    *   "Auditoria (Auditing)"
    *   "Validação (Validation)"
    *   "Autorização (Authorization)"
    *   "Unit of Work (UOW)"
*   A "Observabilidade com OpenTelemetry" também pode ser destacada como uma preocupação transversal, com sua instrumentação abrangendo todas as camadas.

**Estilo Visual Sugerido:**

*   Clara distinção e separação entre as camadas.
*   Componentes e extensões específicas do EAF devem ser visualmente destacados (ex: através de cores diferentes, agrupamento ou rótulos claros) para diferenciá-los dos componentes base do ABP.
*   Setas devem indicar o fluxo primário de controle e dependências (geralmente de cima para baixo, com a Camada de Infraestrutura implementando interfaces das camadas superiores e conectando-se a serviços externos).

---

**Nota sobre Diagramas Adicionais:**

Embora o diagrama acima forneça uma visão geral da arquitetura, diagramas mais detalhados podem ser úteis em seções específicas da documentação para ilustrar fluxos ou interações particulares. Por exemplo:

*   **Fluxo de Autenticação Detalhado:** Um diagrama de sequência mostrando o processo de autenticação via Azure AD ou LDAP, detalhando a interação entre a camada de apresentação, os middlewares do EAF e os provedores de identidade externos.
*   **Coleta e Exportação de Telemetria:** Um diagrama ilustrando como o `Eaf.OpenTelemetry` coleta dados (traces, métricas, logs) das várias camadas da aplicação e os envia para plataformas de monitoramento e análise.

Esses diagramas mais específicos podem ser incorporados nas páginas que detalham os respectivos módulos ou funcionalidades.

## Próximos Passos

As páginas subsequentes nesta seção de arquitetura irão aprofundar em aspectos específicos, como a utilização da base do ASP.NET Boilerplate, as extensões desenvolvidas no EAF, a aplicação de Domain-Driven Design (DDD) e o uso de Injeção de Dependência. Entender esses componentes é crucial para desenvolver e manter aplicações EAF de forma eficaz.
