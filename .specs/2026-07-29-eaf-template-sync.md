# EAF Template Sync — correções dos erros GameHub

> Especificação separada para aplicação das mesmas correncias no template EAF (`afonsoft/EAF`).

## Objetivo

Manter o template Angular/EAF alinhado com as correções aplicadas no GameHub, evitando que novos projetos gerados a partir do template reproduzam os mesmos erros:

- CORS para SignalR (`X-SignalR-User-Agent`).
- Rewrite IIS servindo `index.html` para assets faltantes.
- `m-switch` duplicado por causa do label span.
- `aria-hidden="true"` estático em modais.

## Mudanças necessárias no template EAF

### 1. CORS — `Eaf.Middleware.Web.Core`

**Arquivo:** `src/Eaf.Middleware.Web.Core/Configuration/EafCorsConfiguration.cs`

Na chamada `.WithHeaders(...)` incluir:

```csharp
"X-SignalR-User-Agent"
```

Isso faz com que `AddEafCors` já permita o header customizado do cliente SignalR sem necessidade de post-configuração no projeto consumidor.

### 2. WOFF2 / assets — `web.config` do template Angular

**Arquivo:** `Templates/Angular/Eaf.ProjectName.UI/src/web.config`

Adicionar condição de rewrite para não reescrever assets estáticos com extensão conhecida:

```xml
<add input="{REQUEST_URI}" pattern="^.*\.(woff2?|eot|ttf|svg|png|jpg|jpeg|webp|gif|ico|js|css|json|map|ani|cur)(\?.*)?$" negate="true"/>
```

### 3. m-switch duplicado — `styles.css`

**Arquivo:** `Templates/Angular/Eaf.ProjectName.UI/src/assets/common/styles/styles.css`

Adicionar após o bloco `.m-switch-label`:

```css
.m-switch input:empty ~ span.m-switch-label {
    position: static;
    float: none;
    display: inline-block;
    width: auto;
    height: auto;
    line-height: inherit;
    text-indent: 0;
    cursor: default;
}

.m-switch input:empty ~ span.m-switch-label:before,
.m-switch input:empty ~ span.m-switch-label:after {
    content: none !important;
    display: none !important;
}
```

### 4. aria-hidden em modais

**Arquivos:** `Templates/Angular/Eaf.ProjectName.UI/src/app/**/*-modal.component.html`

Remover `aria-hidden="true"` do `div` raiz de cada modal (`class="modal fade"` com `bsModal`).

## Validação

1. Build da solução EAF (`dotnet build Eaf.sln`).
2. Build do template Angular (`npx ng build --configuration=production`).
3. Verificar no `dist` que `Inter-roman.var.*.woff2` está presente e `web.config` não reescreve extensões estáticas.
4. Testar visualmente `m-switch` em página de configurações/notificações.

## Nota

Após merge no EAF, o GameHub pode remover o post-configure de CORS e voltar a usar `AddEafCors` diretamente, desde que atualize a referência do pacote `Eaf.Middleware.Web.Core` para a versão que contiver essa correção.
