# Prompt: Aplicar correções do GameHub no template EAF

> Resumo das correções de Angular/API identificadas no projeto GameHub para serem replicadas no template principal `afonsoft/EAF`.

---

## Contexto

Você está corrigindo um projeto gerado a partir do template EAF (`afonsoft/EAF`) que apresenta:

- Layout quebrado nos switches/checkboxes do admin (`Settings`).
- Textos localizados exibidos como raw keys (`UseCaptchaOnLogin`).
- Caracteres acentuados corrompidos (`Português` vira `Portugu\u00c3\u00a7s`).
- Itens de menu/rotas legados (`Airplanes`, `Parameters`) e acesso a `Hangfire` quando desabilitado, causando erros 503/404 no console de rede.

Aplique as correções abaixo no template base. Inglês para código, português para documentação/commits.

---

## 1. Angular — Layout dos switches no admin (`styles.css`)

No admin Angular, localize o arquivo `src/assets/common/styles/styles.css` e ajuste a classe `.m-switch-label` para que o label n\u00e3o quebre para baixo do toggle flutuante e n\u00e3o sobreponha t\u00edtulos pr\u00f3ximos.

```css
.m-switch-label {
    display: inline-block;
    margin: 0 0 0 12px;
    vertical-align: top;
    line-height: 34px;
    width: calc(100% - 75px);
}
```

**Valida\u00e7\u00e3o:** abrir a tela `Settings` no admin e verificar que labels longos (ex.: "Consentimento de cookies ativado") ficam alinhados \u00e0 direita do switch sem sobreposi\u00e7\u00e3o.

---

## 2. Angular — Remover menus/rotas legados do template

No template EAF os projetos gerados ainda carregam itens de exemplo que n\u00e3o existem no GameHub. Remova-os:

### `src/app/shared/layout/nav/app-navigation.service.ts`

Remova os itens `Airplanes`, `Parameters` e `Hangfire` de `getMenu()` e `getAdminMenu()`. Mantenha apenas `Dashboard`, `Tenants`, `Roles`, `Users`, `Languages`, `AuditLogs`, `VisualSettings`, `Maintenance` e `Settings`.

### `src/app/admin/admin-routing.module.ts`

Remova a rota `hangfire` e a importa\u00e7\u00e3o do `HangfireComponent`.

### `src/app/admin/admin.module.ts`

Remova a importa\u00e7\u00e3o e a declara\u00e7\u00e3o do `HangfireComponent`.

### `src/app/admin/hangfire/`

Remova a pasta `hangfire` e seus arquivos, ou, se a funcionalidade for necess\u00e1ria futuramente, torne o menu/rotas condicionais \u00e0 configura\u00e7\u00e3o `Hangfire:IsEnabled`.

**Valida\u00e7\u00e3o:** build do admin sem erros (`npm ci && npm run build`) e nenhum request para `/hangfire` ou `/app/admin/parameters` ao navegar no menu.

---

## 3. Localiza\u00e7\u00e3o — Adicionar chaves ausentes no dicion\u00e1rio do projeto

O pipe `| localize` busca primeiro no dicion\u00e1rio do projeto (ex.: `<ProjectName>.xml`). Se a chave n\u00e3o existir, ele faz fallback para `EafCore`, `Abp`, etc., mas algumas chaves usadas pela tela `Settings` est\u00e3o ausentes at\u00e9 mesmo no `EafCore`. Adicione-as no dicion\u00e1rio do projeto gerado:

### `src/<ProjectName>.Core/Application/Localization/<ProjectName>/<ProjectName>.xml`

```xml
<text name="UseCaptchaOnLogin">Use Captcha On Login</text>
<text name="ReCaptcha">ReCaptcha</text>
```

### `src/<ProjectName>.Core/Application/Localization/<ProjectName>/<ProjectName>-pt-BR.xml`

```xml
<text name="UseCaptchaOnLogin">Usar ReCaptcha no Login</text>
<text name="ReCaptcha">ReCaptcha</text>
```

Repita para cada idioma suportado pelo template.

**Valida\u00e7\u00e3o:** abrir `Settings > User Management` e confirmar que o t\u00edtulo do switch aparece traduzido, n\u00e3o como `UseCaptchaOnLogin`.

---

## 4. Encoding — Garantir UTF-8 em arquivos C# com strings localizadas

Arquivos .cs que cont\u00eam strings acentuadas (ex.: `Portugu\u00eas (Brasil)`, `Espa\u00f1ol`) podem estar salvos como Latin-1/ISO-8859-1 ap\u00f3s gera\u00e7\u00e3o do template. Converta-os para UTF-8:

- `src/<ProjectName>.EntityFrameworkCore/Migrations/Seed/Host/DefaultLanguagesCreator.cs`
- `test/<ProjectName>.Tests/Localization/Localization_Tests.cs` (ou equivalente)

Ap\u00f3s a convers\u00e3o, `Português (Brasil)` e `Español` devem ser exibidos corretamente tanto no c\u00f3digo quanto na lista de idiomas do admin.

**Valida\u00e7\u00e3o:** `file -i <arquivo>` deve retornar `charset=utf-8` e o teste de localization deve passar.

---

## 5. Backend — Seed de idiomas deve atualizar registros existentes

O seeder padr\u00e3o s\u00f3 insere idiomas se n\u00e3o existirem, n\u00e3o corrigindo `DisplayName` j\u00e1 corrompido em bases existentes. Ajuste `DefaultLanguagesCreator.Create()` para fazer *upsert* do `DisplayName` e `Icon`:

```csharp
public void Create()
{
    foreach (var language in InitialLanguages)
    {
        var existingLanguage = _context.Languages.IgnoreQueryFilters()
            .FirstOrDefault(l => l.TenantId == language.TenantId && l.Name == language.Name);

        if (existingLanguage != null)
        {
            existingLanguage.DisplayName = language.DisplayName;
            existingLanguage.Icon = language.Icon;
        }
        else
        {
            _context.Languages.Add(language);
        }

        _context.SaveChanges();
    }
}
```

**Valida\u00e7\u00e3o:** executar o projeto com uma base j\u00e1 populada e verificar que `AbpLanguages.DisplayName` para `pt-BR` e `es` fica correto.

---

## 6. Hangfire — Guardar rota/dashboard quando desabilitado

No template, o admin costuma abrir `/hangfire` automaticamente ao clicar em `Jobs`. Se `Hangfire__IsEnabled=false` (Docker Compose padr\u00e3o), o middleware n\u00e3o registra o dashboard e o request retorna 503/404.

Op\u00e7\u00f5es:

- Remover o menu `Jobs` e a rota `/admin/hangfire` quando `Hangfire:IsEnabled` for `false`; ou
- Tornar `HangfireComponent` condicional, consultando `AppConsts`/`Configuration` antes de abrir `/hangfire`.

N\u00e3o deixe links/rotas que apontem para um dashboard inativo.

**Valida\u00e7\u00e3o:** com `Hangfire__IsEnabled=false`, nenhum request para `/hangfire` deve ser emitido ao carregar o admin.

---

## 7. Testes — Web test module n\u00e3o deve duplicar depend\u00eancias

No template, o m\u00f3dulo de testes web (`<ProjectName>WebTestModule`) pode declarar `DependsOn` duas vezes o mesmo m\u00f3dulo (`<ProjectName>TestModule`), fazendo com que `Initialize` (e registro de `DbContextOptions`) seja executado duas vezes e cause `ComponentRegistrationException`.

- Remova duplicatas no `DependsOn`.
- Adicione guarda em `RegisterFakeService` para n\u00e3o registrar componentes j\u00e1 existentes:

```csharp
private void RegisterFakeService<TService>() where TService : class
{
    if (IocManager.IocContainer.Kernel.HasComponent(typeof(TService)))
        return;

    IocManager.IocContainer.Register(
        Component.For<TService>()
            .UsingFactoryMethod(() => Substitute.For<TService>())
            .LifestyleSingleton()
    );
}
```

**Valida\u00e7\u00e3o:** `dotnet test` deve executar sem `Castle.MicroKernel.ComponentRegistrationException`.

---

## Checklist final no template

- [ ] `npm ci && npm run build` no admin sucesso.
- [ ] `dotnet build` do backend sucesso.
- [ ] `dotnet test` passa (sem testes quebrando por duplicidade de DI).
- [ ] Tela `Settings` sem labels quebrados nem raw keys.
- [ ] Menu admin sem `Airplanes`, `Parameters` e `Hangfire` (se desabilitado).
- [ ] Idiomas `pt-BR` e `es` exibidos corretamente.
- [ ] Nenhum request para `/hangfire` ou `/parameters` quando desabilitados.

---

*Prompt gerado a partir das corre\u00e7\u00f5es aplicadas no reposit\u00f3rio `afonsoft/gamehub`.*
