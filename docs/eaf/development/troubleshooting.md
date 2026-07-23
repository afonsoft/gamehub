

# Solução de Problemas em Aplicações EAF

Este documento fornece soluções para problemas comuns encontrados durante o desenvolvimento de aplicações com o Enterprise Application Framework (EAF).

## 1. Problemas de Banco de Dados

*   **Problema**: Não é possível conectar ao banco de dados.
    *   **Solução**: Verifique a string de conexão em `appsettings.json` e certifique-se de que o servidor de banco de dados está em execução e acessível.

*   **Problema**: Migrations falham.
    *   **Solução**: Certifique-se de que as ferramentas do Entity Framework Core estão instaladas e que as migrations estão atualizadas.
    ```bash
    dotnet ef database update -p src/YourProject.EntityFrameworkCore -s src/YourProject.Web
    ```

## 2. Problemas de Autenticação

*   **Problema**: Usuários não conseguem fazer login.
    *   **Solução**: Verifique as credenciais do usuário e certifique-se de que a conta está habilitada. Confira a configuração de autenticação em `appsettings.json`.

*   **Problema**: Usuários não estão autorizados a acessar determinados recursos.
    *   **Solução**: Verifique os roles e permissões do usuário e certifique-se de que os atributos de autorização estão configurados corretamente.

*   **Problema**: Azure AD ou LDAP não funciona.
    *   **Solução**: Verifique as configurações em `appsettings.json` para `AzureActiveDirectory` ou `Ldap`. Confirme que o `TenantId`, `ClientId` e `Authority` estão corretos.

## 3. Erros de Aplicação

*   **Problema**: A aplicação lança uma exceção.
    *   **Solução**: Verifique os logs da aplicação para mensagens detalhadas de erro e stack traces. Use um debugger para percorrer o código e identificar a origem da exceção.

*   **Problema**: A aplicação não responde.
    *   **Solução**: Verifique os logs para erros ou operações de longa duração. Use um profiler para identificar gargalos de performance.

## 4. Problemas de Injeção de Dependência

*   **Problema**: Não é possível resolver uma dependência.
    *   **Solução**: Certifique-se de que a dependência está registrada no container de injeção de dependência (Castle Windsor) e que o lifetime correto está especificado. No ABP, use `ITransientDependency`, `ISingletonDependency` ou registro manual no `PreInitialize()`.

## 5. Problemas de Cache

*   **Problema**: Dados em cache não estão sendo atualizados.
    *   **Solução**: Verifique a configuração do cache e certifique-se de que a invalidação está funcionando corretamente. Para SQL Server Cache, verifique a tabela de cache no banco de dados.

## 6. Problemas de Multi-Tenancy

*   **Problema**: Dados de tenant não estão isolados.
    *   **Solução**: Verifique se todas as entidades implementam `IMayHaveTenant` ou `IMustHaveTenant` e se o ID do tenant está sendo filtrado corretamente nas consultas ao banco.

## 7. Problemas de Logging

*   **Problema**: Nenhum log está sendo gerado.
    *   **Solução**: Verifique a configuração de logging em `appsettings.json`. Para Serilog, confirme que o `MinimumLevel` está configurado adequadamente e que os sinks estão corretos.

## 8. Problemas de Build e Testes

*   **Problema**: Build falha com erros de ambiguidade.
    *   **Solução**: Métodos de extensão do EAF podem conflitar com métodos do ABP. Use qualificação completa do namespace (ex: `Eaf.Middleware.CollectionExtensions.CollectionExtensions.RemoveAll()`).

*   **Problema**: Testes falham no Linux (ex: caminhos com `\`).
    *   **Solução**: Use `Path.DirectorySeparatorChar` em vez de caracteres literais de separador de caminho. Testes com caminhos Windows podem falhar no Linux.

*   **Problema**: Cobertura de código não é gerada.
    *   **Solução**: Certifique-se de usar `--collect:"XPlat Code Coverage"` e que o `coverlet.runsettings` existe na raiz do projeto.

## 9. Problemas de Performance

*   **Problema**: A aplicação está lenta.
    *   **Solução**: Use um profiler para identificar gargalos. Verifique as consultas ao banco e a configuração de cache. Use OpenTelemetry (`Eaf.OpenTelemetry`) para monitoramento.

## 10. Vulnerabilidades Conhecidas

*   **AutoMapper 14.0.0**: A versão 14.0.0 do `AutoMapper` contém uma vulnerabilidade de alta severidade (DoS via recursão não controlada em grafos cíclicos — [GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x)). Não é possível atualizar para o `AutoMapper` 15.1.1+ sem quebrar o `Abp.AutoMapper` 10.4.0, pois o AutoMapper 15+ removeu o construtor `MapperConfiguration(Action<IMapperConfigurationExpression>)` usado internamente pelo ABP. Manteve-se o `AutoMapper` 14.0.0 para preservar a compatibilidade com a ABP 10.4.0. A correção definitiva depende de uma futura versão do `Abp.AutoMapper` que seja compatível com o AutoMapper 15+.

## 11. Dicas Gerais

*   **Verifique os Logs**: Os logs da aplicação são seu primeiro recurso para solução de problemas.
*   **Use um Debugger**: Use um debugger para percorrer o código e identificar a origem dos problemas.
*   **Consulte a Documentação**: Busque na documentação do EAF e do ABP por soluções.
*   **Peça Ajuda**: Se não encontrar solução, abra uma issue no repositório do GitHub.
