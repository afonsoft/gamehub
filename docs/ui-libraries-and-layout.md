# UI Libraries e Padrões de Layout — GameHub Angular Admin

> Documento de referência sobre as bibliotecas de UI, grid, tabelas, formulários e padrões visuais usados no `angular-admin/GameHub.UI`. Baseado no template EAF Angular (`afonsoft/EAF/Templates/Angular/Eaf.ProjectName.UI`).

## Stack Frontend

| Tecnologia | Versão / Fonte | Uso principal |
|------------|----------------|---------------|
| Angular | 20.x | Framework SPA |
| TypeScript | 5.8 | Linguagem |
| Node.js | 20.20.0+ | Runtime de build |
| Angular CLI | 20.x | Build / serve / test |
| Metronic | 5/6 (theme legado) | Layout visual, portlets, header, sidebar, temas |
| Bootstrap | 4.1.3 (via `style.bundle.css` e `vendors.bundle.css`) | Grid system, utilitários, botões, formulários |
| PrimeNG | 17.17.0+ | Tabelas, paginação, editor, autocomplete, upload, árvore |
| ngx-bootstrap | 12.0.0 | Modal, tabs, dropdown, tooltip, popover, datepicker |
| @ng-select/ng-select | 12.0.0 | Combos avançados |
| angular-calendar / @fullcalendar/core | 5.11.3 | Calendários |
| @swimlane/ngx-charts | 20.0.0 | Gráficos |
| chart.js | 4.4.7 | Gráficos (canvas) |
| @microsoft/signalr | 7.0.14 | Tempo real |

## Bibliotecas de Ícones

| Biblioteca | Classes / Arquivos | Uso |
|------------|--------------------|-----|
| FontAwesome 5 | `fa`, `fa-*` | Ícones gerais (`fa-save`, `fa-cog`, `fa-plus`) |
| LineAwesome | `la`, `la-*` | Ícones do tema (`la la-floppy-o`) |
| Flaticon | `flaticon-*` | Ícones do Metronic (`flaticon-settings-1`, `flaticon-search-1`) |
| Simple Line Icons | `icon-*` | Ícones leves |
| PrimeIcons | `pi`, `pi-*` | Ícones dos componentes PrimeNG |
| Bootstrap Icons | `bi`, `bi-*` | Ícones extras |
| Material Symbols | `material-symbols-outlined` | Ícones Material carregados via Google Fonts |

## Grid e Layout

### Bootstrap 4 Grid

Todo o layout responsivo é baseado no grid do Bootstrap 4:

```html
<div class="row">
  <div class="col-xl-6 col-md-12">
    <!-- conteúdo -->
  </div>
  <div class="col-xl-6 col-md-12">
    <!-- conteúdo -->
  </div>
</div>
```

- `.container`, `.container-fluid`
- `.row`, `.no-gutters`
- `.col`, `.col-auto`, `.col-sm-*`, `.col-md-*`, `.col-lg-*`, `.col-xl-*`
- Helpers: `.align-items-center`, `.justify-content-between`, `.text-right`, `.mt-3`, `.mb-3`

### Metronic Layout

O esqueleto das páginas segue o tema Metronic:

```html
<div [@routerTransition]>
  <div class="m-subheader">
    <div class="row align-items-center">
      <div class="mr-auto col-auto">
        <h3 class="m-subheader__title m-subheader__title--separator">
          <span>{{ 'Payments' | localize }}</span>
        </h3>
        <span class="m-section__sub">{{ 'PaymentsHeaderInfo' | localize }}</span>
      </div>
      <div class="col text-right mt-3 mt-md-0">
        <button class="btn btn-primary">Ação</button>
      </div>
    </div>
  </div>

  <div class="m-content">
    <div class="m-portlet m-portlet--mobile">
      <div class="m-portlet__head">
        <div class="m-portlet__head-tools">
          <button class="btn btn-primary"><i class="flaticon-add"></i> {{ 'Create' | localize }}</button>
        </div>
      </div>
      <div class="m-portlet__body">
        <!-- conteúdo da página -->
      </div>
    </div>
  </div>
</div>
```

**Classes principais do Metronic:**

- `.m-subheader` — cabeçalho da página com título e subtítulo
- `.m-subheader__title`, `.m-subheader__title--separator` — título estilizado
- `.m-section__sub` — subtítulo
- `.m-content` — conteúdo principal
- `.m-portlet`, `.m-portlet--mobile` — card/painel da página
- `.m-portlet__head`, `.m-portlet__head-tools` — cabeçalho do portlet com botões
- `.m-portlet__body` — corpo do portlet
- `.m-form` — formulário estilizado
- `.m-form--label-align-right` — alinha labels à direita
- `.m-form__group` — grupo de campo Metronic
- `.m-switch` — checkbox estilizado como switch
- `.m-switch--icon-check` — switch com ícone de check
- `.m-switch-label` — texto ao lado do switch
- `.m-checkbox`, `.m-checkbox-list` — checkboxes estilizados
- `.m-tabs__item` — item de aba Metronic
- `.m-badge`, `.m-badge--success`, `.m-badge--metal`, `.m-badge--wide` — badges
- `.m--margin-bottom-10`, `.m--margin-top-20`, `.m--margin-bottom-20` — helpers de margem

## Tabelas

As tabelas são implementadas com **PrimeNG `p-table`** e paginação com **`p-paginator`**.

```html
<div class="primeng-datatable-container" [busyIf]="dataTableHelper.isLoading">
  <p-table
    #dataTable
    [value]="dataTableHelper.records"
    [lazy]="true"
    [paginator]="false"
    [loading]="dataTableHelper.isLoading"
    (onLazyLoad)="getPayments($event)"
    rows="{{ dataTableHelper.defaultRecordsCountPerPage }}"
    scrollable="true"
    ScrollWidth="100%"
  >
    <ng-template pTemplate="header">
      <tr>
        <th scope="col" pSortableColumn="editionId" style="width: 15%">
          {{ 'Edition' | localize }}
          <p-sortIcon field="editionId"></p-sortIcon>
        </th>
        <th scope="col" style="width: 10%">{{ 'Actions' | localize }}</th>
      </tr>
    </ng-template>

    <ng-template pTemplate="body" let-record="$implicit">
      <tr>
        <td>{{ getEditionDisplayName(record.editionId) }}</td>
        <td>
          <button class="btn btn-sm btn-clean btn-icon btn-icon-md" [attr.aria-label]="l('ProcessPayment')">
            <i class="flaticon-cogwheel"></i>
          </button>
        </td>
      </tr>
    </ng-template>
  </p-table>

  <app-empty-state *ngIf="dataTableHelper.totalRecordsCount === 0 && !dataTableHelper.isLoading" [message]="'NoData' | localize"></app-empty-state>

  <div class="primeng-paging-container">
    <p-paginator
      #paginator
      [rows]="dataTableHelper.defaultRecordsCountPerPage"
      [rowsPerPageOptions]="dataTableHelper.predefinedRecordsCountPerPage"
      [totalRecords]="dataTableHelper.totalRecordsCount"
      (onPageChange)="getPayments($event)"
    ></p-paginator>
    <span class="total-records-count">{{ 'TotalRecordsCount' | localize: dataTableHelper.totalRecordsCount }}</span>
  </div>
</div>
```

### Helpers de tabela

- `DataTableHelper` (`src/shared/helpers/DataTableHelper.ts`) — centraliza paginação, ordenação e loading
- `getSorting(table)`, `getSkipCount(paginator, event)`, `getMaxResultCount(paginator, event)`, `shouldResetPaging(event)`
- `busyIf` directive (`src/shared/utils/busy-if.directive.ts`) — overlay de loading
- `app-empty-state` — estado vazio
- `app-status-badge` — badges booleanos (`trueLabel`, `falseLabel`, `trueClass`, `falseClass`)

### Menu de ações em tabelas

Dropdowns de ações usam **ngx-bootstrap Dropdown**:

```html
<div class="btn-group dropdown" dropdown>
  <button dropdownToggle class="dropdown-toggle btn btn-sm btn-primary">
    <i class="fa fa-cog"></i><span class="caret"></span>
  </button>
  <ul class="dropdown-menu" *dropdownMenu>
    <li><a href="javascript:;" (click)="edit(record)">{{ 'Edit' | localize }}</a></li>
  </ul>
</div>
```

## Formulários

### Formulários template-driven

A maioria dos formulários usa `ngModel` com classes do Bootstrap/Metronic:

```html
<form autocomplete="off">
  <div class="m-form m-form--label-align-right">
    <div class="form-group m-form__group">
      <label for="FieldName">{{ 'FieldName' | localize }}</label>
      <input id="FieldName" name="FieldName" class="form-control" [(ngModel)]="model.field" type="text" />
    </div>

    <div class="form-group m-form__group">
      <span class="m-switch m-switch--icon-check">
        <label>
          <input type="checkbox" name="MySwitch" [(ngModel)]="model.enabled" />
          <span></span>
          <span class="m-switch-label">{{ 'Enable' | localize }}</span>
        </label>
      </span>
    </div>
  </div>
</form>
```

### Agrupamento por abas

Configurações complexas costumam usar `tabset` do **ngx-bootstrap** com estilo Metronic:

```html
<tabset class="tab-container tabbable-line" *ngIf="settings">
  <tab heading="{{ 'General' | localize }}" customClass="m-tabs__item">
    <!-- conteúdo -->
  </tab>
  <tab heading="Stripe" customClass="m-tabs__item">
    <!-- conteúdo -->
  </tab>
</tabset>
```

### Componentes de formulário adicionais

| Componente | Biblioteca | Uso |
|------------|------------|-----|
| `p-autoComplete` | PrimeNG | Autocomplete |
| `p-editor` | PrimeNG (Quill) | Editor de rich text |
| `p-fileUpload` | PrimeNG | Upload de arquivos |
| `p-inputMask` | PrimeNG | Máscaras de entrada |
| `p-tree` | PrimeNG | Árvore (ex.: unidades organizacionais) |
| `p-contextMenu` | PrimeNG | Menu de contexto |
| `ng-select` | @ng-select/ng-select | Combos com busca |
| `bs-datepicker` | ngx-bootstrap | Seletor de data |
| `ngx-file-drop` | ngx-file-drop | Drop zone |
| `ngx-mask` | ngx-mask | Máscaras diversas |
| `ngx-ui-switch` | ngx-ui-switch | Switch alternativo |
| `validation-messages` | EAF | Mensagens de validação por campo |

### Validação

- `required`, `email`, `maxlength`, `minlength` nas diretivas Angular
- `#fieldInput="ngModel"` + `<validation-messages [formCtrl]="fieldInput"></validation-messages>`
- `.ng-touched.ng-invalid` + `.form-md-line-input` para estilização de erro

## Modais

Modais usam **ngx-bootstrap Modal** (`ModalDirective` / `bsModal`):

```html
<div bsModal #myModal="bs-modal" class="modal fade" tabindex="-1" [config]="{ backdrop: 'static' }">
  <div class="modal-dialog modal-lg">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">{{ 'Title' | localize }}</h5>
        <button type="button" class="close" (click)="close()" [attr.aria-label]="l('Close')">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>
      <div class="modal-body">
        <!-- formulário -->
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" (click)="close()">{{ 'Cancel' | localize }}</button>
        <button type="button" class="btn btn-primary" [disabled]="saving" (click)="save()">
          <i class="fa fa-spinner fa-spin" *ngIf="saving"></i> {{ 'Save' | localize }}
        </button>
      </div>
    </div>
  </div>
</div>
```

- `.modal-lg` para modais grandes
- `.modal-dialog` com scroll interno quando necessário
- `[config]="{ backdrop: 'static' }"` para evitar fechar ao clicar fora

## Botões, Badges e Ícones

### Botões Bootstrap / Metronic

```html
<button class="btn btn-primary"><i class="flaticon-add"></i> {{ 'Create' | localize }}</button>
<button class="btn btn-info"><i class="flaticon-settings-1"></i> {{ 'GatewaySettings' | localize }}</button>
<button class="btn btn-secondary">{{ 'Cancel' | localize }}</button>
<button class="btn btn-metal">{{ 'UseSystemDefaults' | localize }}</button>
<button class="btn btn-sm btn-clean btn-icon btn-icon-md"><i class="flaticon-cogwheel"></i></button>
```

### Badges

```html
<span class="badge badge-warning">{{ 'Pending' | localize }}</span>
<span class="badge badge-success">{{ 'Completed' | localize }}</span>
<span class="badge badge-info">{{ 'Processing' | localize }}</span>
<span class="badge badge-secondary">{{ 'Canceled' | localize }}</span>
<span class="badge badge-danger">{{ 'Failed' | localize }}</span>
```

Também há badges Metronic:

```html
<span class="m-badge m-badge--success m-badge--wide">{{ trueLabel }}</span>
<span class="m-badge m-badge--metal m-badge--wide">{{ falseLabel }}</span>
```

## Temas e Estilos Globais

Arquivos principais configurados em `angular-admin/GameHub.UI/angular.json`:

```json
"styles": [
  "node_modules/animate.css/animate.min.css",
  "node_modules/quill/dist/quill.core.css",
  "node_modules/quill/dist/quill.snow.css",
  "node_modules/famfamfam-flags/dist/sprite/famfamfam-flags.css",
  "node_modules/angular-calendar/css/angular-calendar.css",
  "node_modules/@ng-select/ng-select/themes/default.theme.css",
  "node_modules/primeng/resources/themes/lara-light-blue/theme.css",
  "node_modules/primeicons/primeicons.css",
  "node_modules/sweetalert2/dist/sweetalert2.css",
  "node_modules/cookieconsent/build/cookieconsent.min.css",
  "src/assets/lib/freezeUI/freeze-ui.min.css",
  "src/assets/lib/primeng/file-upload/css/primeng.file-upload.css",
  "src/assets/lib/primeng/autocomplete/css/primeng.autocomplete.css",
  "src/assets/lib/primeng/tree/css/primeng.tree.css",
  "src/assets/lib/primeng/context-menu/css/primeng.context-menu.css",
  "src/assets/common/fonts/fonts-eaf.css",
  "src/assets/lib/ngx-bootstrap/bs-datepicker.css",
  "src/assets/lib/metronic/assets/vendors/base/vendors.bundle.css",
  "src/assets/common/styles/styles.css"
]
```

### Temas por layout

Cada tema possui seu `style.bundle.css` e `customize.css` em `angular-admin/GameHub.UI/src/assets/common/styles/themes/<nome>/`:

- `default`
- `theme2`
- `theme3`
- `theme4`

O tema ativo é gerenciado por `AppUiCustomizationService` e `currentTheme`.

## Animações e Transições

- `[@routerTransition]` — animação de entrada das páginas
- `appModuleAnimation()` — função de animação padrão dos componentes
- `animate.css` — biblioteca de animações CSS

## Padrões Recomendados para Novas Telas

1. **Cabeçalho**: sempre usar `.m-subheader` com título e descrição.
2. **Container**: envolver conteúdo em `.m-content > .m-portlet`.
3. **Filtros**: usar `.m-form.m-form--label-align-right` dentro de `.m-portlet__body`.
4. **Listagens**: usar `p-table` + `p-paginator` + `app-empty-state`.
5. **Configurações agrupadas**: usar `tabset` do ngx-bootstrap com `tabbable-line` e `m-tabs__item`.
6. **Formulários**: combinar `.form-group.m-form__group` com `form-control` e `m-switch`/`m-checkbox`.
7. **Modais**: usar `bsModal` do ngx-bootstrap com `.modal-dialog.modal-lg` e botões `.btn-primary`/`.btn-secondary`.
8. **Ícones**: preferir `flaticon-*` ou `la-*` para manter identidade Metronic.
9. **Responsividade**: usar grid Bootstrap 4 (`col-xl-*`, `col-md-*`, `mt-md-0`, etc.).
10. **Acessibilidade**: incluir `scope="col"` nos cabeçalhos de tabela, `aria-label` em botões de ação e `aria-hidden="true"` em ícones decorativos.

## Módulos Importados em AdminModule

```typescript
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PopoverModule } from 'ngx-bootstrap/popover';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { TooltipModule } from 'ngx-bootstrap/tooltip';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { ContextMenuModule } from 'primeng/contextmenu';
import { EditorModule } from 'primeng/editor';
import { FileUploadModule as PrimeNgFileUploadModule } from 'primeng/fileupload';
import { InputMaskModule } from 'primeng/inputmask';
import { PaginatorModule } from 'primeng/paginator';
import { TableModule } from 'primeng/table';
import { TreeModule } from 'primeng/tree';
```

## Arquivos de Referência no Repositório

- `angular-admin/GameHub.UI/src/app/admin/settings/settings.component.html` — exemplo de configurações com abas
- `angular-admin/GameHub.UI/src/app/admin/ui-customization/default-theme-ui-settings.component.html` — exemplo de form Metronic por aba
- `angular-admin/GameHub.UI/src/app/admin/users/create-or-edit-user-modal.component.html` — exemplo de modal com abas
- `angular-admin/GameHub.UI/src/app/admin/payments/payments.component.html` — exemplo de listagem PrimeNG
- `angular-admin/GameHub.UI/src/app/admin/payments/payment-gateway-settings-modal.component.html` — modal de configuração de gateways
- `angular-admin/GameHub.UI/src/app/shared/components/status-badge/status-badge.component.ts` — componente de badge
- `angular-admin/GameHub.UI/src/app/shared/components/empty-state/empty-state.component.ts` — componente de estado vazio
- `angular-admin/GameHub.UI/src/assets/common/styles/styles.css` — estilos customizados EAF
- `angular-admin/GameHub.UI/src/assets/common/styles/themes/*/style.bundle.css` — bundles de tema
