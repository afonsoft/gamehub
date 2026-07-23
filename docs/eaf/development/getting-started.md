
# Primeiros Passos com o Desenvolvimento EAF

Este guia fornece uma introdução passo a passo para configurar seu ambiente de desenvolvimento e começar a trabalhar com o EAF.

## 1. Pré-requisitos

*   **.NET SDK 10.0**: Certifique-se de ter o .NET SDK instalado (versão 10.0 ou superior). Baixe em [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).
*   **IDE**: Recomendamos o Visual Studio Code ou Visual Studio 2022+.
*   **Git**: Certifique-se de ter o Git instalado.
*   **Banco de Dados**: Você precisará de um servidor de banco de dados (ex: SQL Server, PostgreSQL, SQLite) para sua aplicação EAF.

## 2. Configurando o Ambiente de Desenvolvimento

1.  **Clone o Repositório EAF**:
    ```bash
    git clone https://github.com/afonsoft/EAF.git
    cd EAF
    ```

2.  **Restaure as Dependências**:
    ```bash
    dotnet restore Eaf.sln
    ```

3.  **Compile a Solução**:
    ```bash
    dotnet build Eaf.sln
    ```

## 3. Executando os Testes

1.  **Execute todos os testes com cobertura de código**:
    ```bash
    dotnet test Eaf.sln --collect:"XPlat Code Coverage" --settings coverlet.runsettings
    ```

2.  **Gere o relatório de cobertura** (requer `reportgenerator`):
    ```bash
    dotnet tool install -g dotnet-reportgenerator-globaltool
    reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:"TextSummary;Html"
    ```

3.  **Ou use o script automatizado**:
    ```bash
    # Linux/macOS
    ./run-tests-with-coverage.sh

    # Windows (PowerShell)
    .\run-tests-with-coverage.ps1
    ```

## 4. Estrutura do Projeto

*   `src/` - Código fonte dos módulos de middleware EAF
    *   `Eaf.Middleware.Core` - Módulo core com funcionalidades base compartilhadas
    *   `Eaf.Middleware.Application` - Camada de aplicação (Application Services, DTOs)
    *   `Eaf.Middleware.Web.Core` - Módulo web core (Controllers, autenticação web)
    *   `Eaf.Middleware.Worker` - Módulo worker para serviços de background
    *   `Eaf.Castle.Serilog` - Integração Serilog com Castle Windsor
    *   `Eaf.KeyVault` / `Eaf.KeyVault.AspNetCore` - Azure Key Vault
    *   `Eaf.OpenTelemetry` - Observabilidade com OpenTelemetry
    *   `Eaf.SqlServerCache` / `Eaf.SqliteCache` - Cache distribuído
    *   `Eaf.Log4NetServiceBus` - Logging via Azure Service Bus
    *   `Eaf.Middleware.AzureActiveDirectory` - Autenticação Azure AD
    *   `Eaf.Middleware.Ldap` - Autenticação LDAP
*   `test/` - Projetos de teste (xUnit, Shouldly, NSubstitute)
*   `Templates/` - Templates para novos projetos (API, Angular, Worker)
*   `docs/` - Documentação do projeto

## 5. Padrões de Teste

O EAF segue o padrão BDD (Behavior-Driven Development) com nomenclatura em português:

```csharp
[Fact]
public void Dado_UsuarioValido_Quando_CriarConta_Entao_DeveRetornarSucesso()
{
    // Dado (Arrange)
    var input = new CreateUserInput { Name = "João", Email = "joao@test.com" };

    // Quando (Act)
    var result = _userService.CreateUser(input);

    // Então (Assert)
    result.ShouldNotBeNull();
    result.Name.ShouldBe("João");
}
```

Ferramentas de teste:
- **xUnit** - Framework de testes
- **Shouldly** - Assertions fluentes (ex: `result.ShouldBe(expected)`)
- **NSubstitute** - Mocking de dependências
- **coverlet** - Cobertura de código

## 6. Próximos Passos

*   Explore o código fonte e familiarize-se com a arquitetura.
*   Leia a documentação dos [módulos](../modules/README.md) para entender cada componente.
*   Consulte a [arquitetura](../architecture/README.md) para entender o design do sistema.
*   Comece a criar sua aplicação usando os templates disponíveis em `Templates/`.
