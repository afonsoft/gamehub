# Layout do Angular Admin — GameHub

> Documento de referência sobre a estrutura de layout, responsividade e alinhamento do `angular-admin/GameHub.UI` com o template EAF Angular 9.4.4.

## 1. Visão geral

O admin do GameHub (`angular-admin/GameHub.UI`) é baseado no template Angular do EAF (`Templates/Angular/Eaf.ProjectName.UI`), que utiliza:

- **Angular 20.3.x** (with standalone components disabled for legacy modules).
- **Metronic / Bootstrap 5** para estrutura e grids.
- **PrimeNG** para tabelas, paginadores e componentes de formulário.
- **ngx-bootstrap** para modais e dropdowns.
- **Service Worker** (`ngsw-config.json`) para cache de assets.

A estrutura visual padrão segue o padrão do EAF:

```
m-page
├── m-header
│   └── topbar.component.html (lateral direita: idiomas, notificações, chat, perfil)
├── m-aside-left
│   └── sidebar / menu lateral
└── m-wrapper
    └── m-content
        └── m-subheader + m-portlet (conteúdo das páginas)
```

## 2. Topbar

**Arquivos:**

- `angular-admin/GameHub.UI/src/app/shared/layout/topbar.component.ts`
- `angular-admin/GameHub.UI/src/app/shared/layout/topbar.component.html`

O topo utiliza o componente `TopBarComponent`. A versão 9.4.4 do template EAF trouxe a seguinte evolução (já replicada no GameHub):

- Substituição de `<button>` por `<a href="javascript:;">` nos toggles de dropdown (idiomas, chat e perfil do usuário) para evitar conflitos de comportamento com menus e acessibilidade.
- Remoção do botão mobile toggle da topbar (`m-topbar__menu-toggle`); o controle do menu mobile é feito pelo próprio `m-header-menu-button` do header.
- Remoção de estados manuais `languageDropdownExpanded` e `userDropdownExpanded`; os dropdowns passam a ser gerenciados pelo `m-dropdown-toggle="click"`.
- Remoção do método `toggleMobileMenu()` do `topbar.component.ts`.
- O chat continua abrindo via `showChat('chatSideRight')`, adicionando a classe `mr-0` ao painel lateral.

### Componentes do topbar

| Região | Componente/Tag | Função |
|--------|----------------|--------|
| Idiomas | `m-topbar__languages` | Seletor de idioma quando `languages.length > 1`. |
| Admin | `adm-bar` | Barra de atalhos administrativos. |
| Notificações | `headerNotifications` | Dropdown de notificações. |
| Chat | `chat_is_connecting_icon` | Indicador de conexão do chat e botão para abrir o side panel. |
| Perfil | `m-topbar__user-profile` | Dropdown com ações de usuário (back-to-my-account, change password, login attempts, change picture, my settings, logout). |

## 3. Menu lateral (aside)

O menu lateral é renderizado por `sidebar.component`. Em telas menores:

- A classe `.m-aside-left` recebe `transform: translateX(-100%)` por padrão.
- Quando aberto (`m-aside-left--on`), recebe `transform: translateX(0)`.
- O botão `.m-header-menu-button` é exibido em `max-width: 992px` para controlar o aside.

```css
/* assets/common/styles/styles.css */
@media (max-width: 992px) {
    .m-header-menu-button {
        display: inline-flex;
    }

    .m-aside-left {
        transform: translateX(-100%);
        transition: transform 0.2s ease-in-out;
    }

    .m-aside-left.m-aside-left--on {
        transform: translateX(0);
    }

    .m-body:not(.m-aside-left--none) {
        margin-left: 0 !important;
    }
}
```

## 4. Service Worker e cache de assets

**Arquivo:** `angular-admin/GameHub.UI/ngsw-config.json`

O cache foi atualizado na versão 9.4.4 para refletir o output-hashing do Angular:

- `app`: prefetch de arquivos essenciais (index, favicon, manifest, `*.css`, `main*.js`, `polyfills*.js`, `runtime*.js`, `scripts*.js`).
- `lazy`: lazy cache de todos os `*.js` fragmentados.
- `assets`: lazy cache de imagens, fontes e assets estáticos.

Isso substitui o padrão antigo baseado em `*.bundle.css`, `*.bundle.js` e `*.chunk.js`.

## 5. Telas de pagamentos

**Arquivos:**

- `angular-admin/GameHub.UI/src/app/admin/payments/payments.component.ts`
- `angular-admin/GameHub.UI/src/app/admin/payments/payments.component.html`
- `angular-admin/GameHub.UI/src/app/admin/payments/payments.component.spec.ts`
- `angular-admin/GameHub.UI/src/app/admin/payments/payment-gateway-settings-modal.component.ts`
- `angular-admin/GameHub.UI/src/app/admin/payments/payment-gateway-settings-modal.component.html`

### 5.1 Listagem (`payments.component`)

- Tabela `p-table` lazy-load com paginação externa via `p-paginator`.
- Filtros por texto (`filterText`) e status (`Pending`, `Completed`, `Canceled`, `Failed`).
- Colunas: Edition, Amount, Status, Gateway, ExternalPaymentId, PaymentTime e Actions.
- Status renderizados com badges via `getStatusClass` e traduzidos via `getStatusLabel`.
- Botão de ação **Process** aparece apenas para pagamentos `Pending` e quando o usuário tem a permissão `Pages.Administration.Payments.Process`.

### 5.2 Criação de pagamento

Modal com campos:

- Edition (select)
- Payment Type (`NewRegistration`, `BuyNow`, `Upgrade`, `Extend`)
- Payment Period (`Daily`, `Weekly`, `Monthly`, `Quarterly`, `Biannual`, `Annual`, `Permanent`)
- Gateway (select carregado de `PaymentServiceProxy.getGatewayList()`)
- Description (textarea)

### 5.3 Processamento manual

Modal `processModal` para preencher `externalPaymentId`, `gateway`, `gatewayResponse` e flag `isSuccess`, enviando via `PaymentServiceProxy.processPayment`.

### 5.4 Configuração de gateways

Modal `payment-gateway-settings-modal` permite editar credenciais dos gateways suportados:

- Stripe
- PayPal
- MercadoPago
- PagSeguro

O componente utiliza `IPaymentGatewaySettingsDto` e reinstancia `PaymentGatewaySettingsDto` antes de enviar para `updateGatewaySettings`, garantindo que a serialização `toJSON()` seja respeitada para DTOs aninhados.

## 6. Responsividade e acessibilidade

### Classes de toque

```css
/* assets/common/styles/styles.css */
.btn,
.m-portlet__nav-link,
.page-link,
.nav-link,
.dropdown-item {
    min-height: 36px;
}
```

### Formulários de login

Inputs e botões do login possuem `min-height: 44px` e `font-size: 16px` para evitar zoom em iOS e garantir targets de toque.

## 7. Alinhamento com EAF 9.4.4

O `angular-admin/GameHub.UI` já reflete as alterações do template EAF 9.4.4:

| Área | Alteração EAF 9.4.4 | Estado no GameHub |
|------|---------------------|-------------------|
| `topbar.component.html` | Botões viraram anchors; remoção do toggle mobile e estados `aria-expanded` manuais | Aplicado |
| `topbar.component.ts` | Remoção de `languageDropdownExpanded`, `userDropdownExpanded`, `toggleLanguageDropdown`, `toggleUserDropdown`, `toggleMobileMenu` | Aplicado |
| `ngsw-config.json` | Cache por arquivos com hash (`main*.js`, `*.css`, lazy `*.js`) | Aplicado |
| `package.json` | Bump de `@angular/common`, `@angular/compiler`, `@angular/core`, `@angular/platform-server` para `20.3.27` | Aplicado |
| `styles.css` | Limpeza de regras de focus-visible e mobile tweaks removidas do template | Aplicado (as regras removidas não existiam no GameHub) |
| `test-helpers/mock-services.ts` | Ordem dos parâmetros `getEditions` ajustada para `skipCount, maxResultCount` | Aplicado |
| Telas de pagamentos | Sem alterações estruturais no template 9.4.4; HTML e TS mantêm paridade funcional | Alinhado |

## 8. Convenções para manutenção

- Preferir **Bootstrap 5** e utilitários do Metronic para grids e espaçamento.
- Tabelas administrativas devem usar `p-table` com `scrollable="true"` e `ScrollWidth="100%"` para responsividade horizontal.
- Modais devem usar `bsModal` do `ngx-bootstrap` e seguir o padrão de `aria-hidden="true"` no container.
- Manter `package.json` e `package-lock.json` sincronizados com o template EAF após qualquer bump de Angular.
- Ao gerar novos `service-proxies.ts` (via NSwag), revisar mocks em `test-helpers/mock-services.ts` para manter assinaturas compatíveis.

## 9. Referências

- Template EAF Angular: `afonsoft/EAF/Templates/Angular/Eaf.ProjectName.UI`
- Componentes compartilhados: `angular-admin/GameHub.UI/src/app/shared/layout/`
- Estilos comuns: `angular-admin/GameHub.UI/src/assets/common/styles/styles.css`
- Configuração do service worker: `angular-admin/GameHub.UI/ngsw-config.json`
