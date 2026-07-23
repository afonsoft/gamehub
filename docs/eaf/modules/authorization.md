# Autorização no EAF (Enterprise Application Foundation)

## 1. Introdução à Autorização no EAF

A autorização é o processo que determina se um usuário autenticado tem o direito de acessar um recurso específico ou executar uma determinada ação dentro da aplicação. Enquanto a autenticação verifica "quem você é", a autorização verifica "o que você tem permissão para fazer".

O EAF (Enterprise Application Foundation) é uma plataforma open source que utiliza o robusto sistema de autorização baseado em permissões do ASP.NET Boilerplate (ABP), que é uma funcionalidade central fornecida por módulos como o `AbpZeroCoreModule`. Este sistema é altamente configurável e permite um controle de acesso granular sobre as funcionalidades da aplicação.

## 2. Conceitos Centrais de Autorização

O sistema de autorização do ABP, e por extensão do EAF, gira em torno de alguns conceitos fundamentais:

### 2.1. Permissões (Permissions)

Permissões representam o direito de executar uma ação específica ou acessar uma funcionalidade. Elas são a base do sistema de autorização.

*   **Definindo Permissões**:
    As permissões são definidas em classes que herdam de `AuthorizationProvider`. Essas classes são tipicamente localizadas no módulo Core do EAF ou em módulos específicos que introduzem novas funcionalidades. As permissões podem ser hierárquicas.

    **Exemplo EAF (Definição de Permissões para um Módulo de Projetos)**:
    ```csharp
    // No projeto Eaf.MyModule.Core, em um arquivo como MyModulePermissions.cs (constantes)
    public static class MyModulePermissions
    {
        public const string GroupName = "Eaf.MyModule"; // Nome do grupo de permissões

        // Permissões para a entidade "CustomProject"
        public const string Projects = GroupName + ".Projects"; // Permissão raiz para projetos
        public const string Projects_View = Projects + ".View";
        public const string Projects_Create = Projects + ".Create";
        public const string Projects_Edit = Projects + ".Edit";
        public const string Projects_Delete = Projects + ".Delete";
        public const string Projects_Approve = Projects + ".Approve";
    }

    // No projeto Eaf.MyModule.Core, em um arquivo como MyModuleAuthorizationProvider.cs
    using Abp.Authorization;
    using Abp.Localization; // Para L()

    public class MyModuleAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            var myModuleGroup = context.CreatePermission(
                MyModulePermissions.GroupName, L("MyModulePermissions_GroupName") // Localizável
            );

            var projectsPermission = myModuleGroup.CreateChildPermission(
                MyModulePermissions.Projects, L("Projects")
            );
            projectsPermission.CreateChildPermission(MyModulePermissions.Projects_View, L("ViewProjects"));
            projectsPermission.CreateChildPermission(MyModulePermissions.Projects_Create, L("CreateProjects"));
            projectsPermission.CreateChildPermission(MyModulePermissions.Projects_Edit, L("EditProjects"));
            projectsPermission.CreateChildPermission(MyModulePermissions.Projects_Delete, L("DeleteProjects"));
            projectsPermission.CreateChildPermission(MyModulePermissions.Projects_Approve, L("ApproveProjects"), isGrantedByDefault: false); // Exemplo de permissão não concedida por padrão
        }

        private static ILocalizableString L(string name)
        {
            // Supondo que a fonte de localização "MyModuleSource" está definida em algum lugar
            return new LocalizableString(name, "MyModuleSource");
        }
    }
    ```
    Esta classe `MyModuleAuthorizationProvider` deve ser registrada no método `PreInitialize` do módulo correspondente: `Configuration.Authorization.Providers.Add<MyModuleAuthorizationProvider>();`.

*   **Verificando Permissões**:
    O serviço `IPermissionChecker` é usado para verificar se o usuário atual tem uma permissão específica. Ele é injetado e usado tanto declarativamente quanto programaticamente.

### 2.2. Autorização Declarativa (`[AbpAuthorize(...)]`)

Esta é a forma mais comum de aplicar autorização, usando atributos em classes ou métodos.

*   **Exemplos EAF**:
    ```csharp
    // Em um Serviço de Aplicação EAF (Eaf.MyModule.Application)
    using Abp.Application.Services;
    using Abp.Authorization;

    public class CustomProjectAppService : ApplicationService, ICustomProjectAppService
    {
        [AbpAuthorize(MyModulePermissions.Projects_View)] // Requer permissão para visualizar
        public async Task<ProjectDto> GetProjectAsync(Guid id)
        {
            // ... lógica para buscar projeto ...
            return new ProjectDto();
        }

        // Requer permissão para criar OU editar projetos
        [AbpAuthorize(MyModulePermissions.Projects_Create, MyModulePermissions.Projects_Edit)]
        public async Task CreateOrUpdateProjectAsync(CreateOrUpdateProjectDto input)
        {
            // ... lógica ...
        }

        // Exemplo com RequireAllPermissions (se o atributo suportar diretamente ou através de lógica customizada)
        // O padrão do AbpAuthorize com múltiplas strings é OR.
        // Para AND, geralmente se usa múltiplos atributos ou um wrapper customizado.
        // Mas se uma permissão única representa a necessidade de "todas as permissões de um conjunto",
        // isso é gerenciado pela definição da permissão ou pela lógica do papel.
        // Para um cenário onde TODAS as permissões listadas são necessárias:
        [AbpAuthorize(MyModulePermissions.Projects_Approve)] // Supondo que aprovar requer várias sub-permissões implícitas
                                                           // ou que a lógica de negócio verifica múltiplas permissões internamente.
        // Alternativamente, se o atributo fosse [AbpAuthorize(Permissions = new[]{"P1", "P2"}, RequireAll = true)]
        // (Nota: Esta sintaxe exata de 'Permissions' e 'RequireAll' pode variar ou ser customizada no EAF/ABP)
        // O comportamento padrão do [AbpAuthorize("P1", "P2")] é que o usuário precisa ter P1 OU P2.
        // Para requerer P1 E P2, você pode usar múltiplos atributos:
        // [AbpAuthorize(MyModulePermissions.Projects_Edit)]
        // [AbpAuthorize(MyModulePermissions.Projects_Approve)]
        // Ou, mais comumente, definir uma permissão granular que implica ambas.
        public async Task ApproveProjectAsync(Guid id)
        {
             // Se RequireAllPermissions = true for uma propriedade do atributo (como em algumas versões/customizações do ABP):
             // [AbpAuthorize(MyModulePermissions.Projects_Edit, MyModulePermissions.Projects_Approve, RequireAllPermissions = true)]
             // ... lógica para aprovar projeto ...
        }
    }
    ```
    *   `RequireAllPermissions`: Quando `true` (se o atributo for usado com uma coleção de permissões e essa propriedade existir), o usuário deve ter *todas* as permissões listadas. Se `false` (padrão), o usuário precisa de *pelo menos uma* das permissões listadas. (Nota: o atributo `AbpAuthorize` padrão do ABP trata múltiplos argumentos de string como um OR. Para um AND lógico, múltiplos atributos são usados, ou uma permissão composta é definida.)

### 2.3. Autorização Programática

Permite verificações de permissão dentro da lógica do método.

*   **Exemplo EAF**:
    ```csharp
    // Em um Serviço de Domínio ou Aplicação EAF
    using Abp.Authorization;
    using Abp.Domain.Repositories; // Para IRepository
    using System; // Para Guid
    using System.Threading.Tasks; // Para Task

    public class ProjectOperationsService : ApplicationService // Ou IDomainService
    {
        private readonly IRepository<CustomProject, Guid> _projectRepository; // Supondo que CustomProject é uma entidade EAF

        public ProjectOperationsService(IRepository<CustomProject, Guid> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task DeleteProjectAsync(Guid projectId)
        {
            if (!await PermissionChecker.IsGrantedAsync(MyModulePermissions.Projects_Delete))
            {
                throw new AbpAuthorizationException("Você não tem permissão para deletar projetos.");
            }
            var project = await _projectRepository.GetAsync(projectId);
            // Lógica adicional, e.g., apenas o criador do projeto pode deletar, mesmo com a permissão geral
            if (project.CreatorUserId != AbpSession.UserId)
            {
                 // Supondo uma permissão mais granular para administradores deletarem qualquer projeto
                 if (!await PermissionChecker.IsGrantedAsync(MyModulePermissions.GroupName + ".Projects.DeleteAsAdmin"))
                 {
                    throw new AbpAuthorizationException("Você só pode deletar projetos que você criou.");
                 }
            }
            await _projectRepository.DeleteAsync(projectId);
        }
    }
    ```

### 2.4. Papéis (Roles)

Papéis são agrupamentos de permissões. Usuários são atribuídos a papéis, e herdam as permissões concedidas a esses papéis.

*   **Tipos de Papéis (ABP Zero)**:
    *   **Static Roles**: Definidos no código (e.g., "Admin", "User"). Suas permissões são geralmente fixas ou gerenciadas via código/migrações.
    *   **Dynamic Roles**: Criados e gerenciados em tempo de execução, tipicamente através de uma UI administrativa. Permissões podem ser concedidas/revogadas dinamicamente.
*   **Atribuição de Permissões a Papéis**: No EAF (baseado em ABP Zero), isso é geralmente feito através da UI de gerenciamento de Papéis, onde um administrador pode marcar quais permissões um papel específico possui. Para papéis estáticos, as permissões iniciais podem ser definidas no `StaticRoleDefinition`.
*   **Atribuição de Usuários a Papéis**: Também gerenciado via UI (gerenciamento de Usuários) ou programaticamente usando `AbpUserManager.AddToRoleAsync(User, roleName)`.

### 2.5. Usuários (Users)

Permissões são concedidas a usuários principalmente através dos papéis aos quais estão associados. O ABP Zero também suporta a concessão ou negação de permissões específicas diretamente a um usuário, o que pode sobrescrever as permissões herdadas dos papéis. Isso é gerenciado pela entidade `UserPermissionSetting`.

## 3. Gerenciamento de Funcionalidades (Features) e Autorização

O ABP fornece um sistema de "Features" que permite habilitar ou desabilitar funcionalidades para diferentes tenants (em aplicações multi-tenant) ou edições. Features podem interagir com o sistema de permissões.

*   **`IFeatureChecker`**: Serviço usado para verificar se uma feature está habilitada.
*   **Interação com Permissões**:
    *   Uma permissão pode ser dependente de uma feature. Se a feature estiver desabilitada, a permissão relacionada também será considerada desabilitada, mesmo que concedida a um papel/usuário.
    *   Isso é definido ao criar a permissão: `context.CreatePermission(..., featureDependency: new SimpleFeatureDependency("MyEafFeature"))`.
*   **Exemplos EAF**:
    ```csharp
    // Definindo uma permissão dependente de feature
    // public override void SetPermissions(IPermissionDefinitionContext context)
    // {
    //    var featureDependentPermission = myModuleGroup.CreateChildPermission(
    //        MyModulePermissions.Projects_AdvancedExport,
    //        L("AdvancedExportProjects"),
    //        featureDependency: new SimpleFeatureDependency("Eaf.AdvancedExportFeature")
    //    );
    // }

    // Verificando feature em um serviço
    using Abp.Application.Features;
    using Abp.Application.Services; // Para ApplicationService
    using Abp.Authorization; // Para AbpAuthorizationException
    using System.Threading.Tasks; // Para Task

    [RequiresFeature("Eaf.ReportingModuleFeature")] // Requer que a feature do módulo de relatórios esteja habilitada
    public class ReportingAppService : ApplicationService
    {
        public async Task<string> GenerateReportAsync() // ReportOutput é um tipo de exemplo
        {
            if (!await FeatureChecker.IsEnabledAsync("Eaf.SpecificReportTypeFeature"))
            {
                throw new AbpAuthorizationException("Tipo de relatório específico não habilitado.");
            }
            // ... gerar relatório ...
            return "Report Data"; // Retornando string para simplificar
        }
    }
    ```

## 4. Considerações de Autorização Específicas do EAF

*   **Convenções de Nomenclatura**: O EAF pode adotar convenções específicas para nomear permissões e features para manter a consistência entre os módulos (e.g., `Eaf.ModuleName.EntityName.Action`).
*   **Mapeamento de Claims Externos para Permissões**: Ao usar provedores de autenticação externos como Azure AD (detalhado em `docs/architecture/eaf-extensions.md`), o EAF pode incluir lógica para mapear claims (e.g., grupos do Azure AD) para papéis ou permissões do ABP durante o processo de login. Isso pode ser feito no evento `OnTokenValidated` do manipulador OpenID Connect ou em um `DefaultExternalAuthenticationSource`.
*   **Permissões por Tenant**: Em um ambiente multi-tenant, as permissões e papéis (especialmente os dinâmicos) são geralmente específicos por tenant. O `AbpZeroCoreModule` lida com essa separação.

## 5. Serviços Chave para Autorização

*   `IPermissionChecker`: O serviço principal para verificar permissões.
*   `IAuthorizationHelper`: Ajuda com cenários de autorização mais complexos, combinando múltiplas verificações.
*   `IPermissionManager`: Gerencia as definições de todas as permissões disponíveis no sistema.
*   `IRolePermissionStore`, `IUserPermissionStore`: Interfaces de infraestrutura (usadas internamente pelo ABP) para persistir e recuperar configurações de permissão para papéis e usuários.

## 6. Troubleshooting de Problemas Comuns de Autorização

*   **Acesso Negado (Erro 403 Forbidden)**:
    *   **Causa Comum**: O usuário autenticado não possui a permissão necessária para o recurso ou ação.
    *   **Verificar**:
        *   O(s) nome(s) da(s) permissão(ões) no atributo `[AbpAuthorize(...)]` ou na chamada programática `PermissionChecker.IsGrantedAsync(...)`.
        *   Os papéis atribuídos ao usuário.
        *   As permissões concedidas a esses papéis (verificar na UI de gerenciamento de papéis do EAF/ABP).
        *   Se há alguma permissão específica de usuário que possa estar negando o acesso.
        *   Se a permissão depende de uma feature que está desabilitada.
*   **Permissões Não Funcionando Como Esperado**:
    *   **Erro de Digitação**: Nomes de permissão devem corresponder exatamente entre a definição, o atributo/verificação, e a concessão no papel/usuário.
    *   **Cache de Permissões**: O ABP cacheia as definições e configurações de permissão. Se você alterar definições ou concessões e não vir o efeito imediatamente, pode ser necessário limpar o cache ou reiniciar a aplicação (especialmente em desenvolvimento).
    *   **Lógica Incorreta na Verificação Programática**: Reveja a lógica condicional onde `IPermissionChecker` é usado.
*   **Problemas com Dependências de Features**:
    *   Uma funcionalidade protegida por uma permissão que depende de uma feature não estará acessível se a feature estiver desabilitada, mesmo que a permissão tenha sido concedida. Verifique o status da feature via `IFeatureChecker` ou na UI de gerenciamento de features/edições.
*   **Problemas com Atribuições de Papel ou Permissão de Usuário**:
    *   Confirme se o usuário está corretamente atribuído aos papéis esperados.
    *   Verifique se as permissões para esses papéis estão configuradas corretamente.
    *   Cuidado com permissões diretas de usuário que podem estar sobrescrevendo ou entrando em conflito com as permissões do papel.

Entender e utilizar corretamente o sistema de autorização do EAF/ABP é crucial para construir aplicações seguras e robustas, garantindo que os usuários só possam acessar as funcionalidades e dados para os quais têm direito.
