import { AfterViewInit, Component, ElementRef, Injector, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { LanguageServiceProxy } from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { EditTextModalComponent } from './edit-text-modal.component';
import { finalize } from 'rxjs/operators';



@Component({
  standalone: false,
  templateUrl: './language-texts.component.html',
  animations: [appModuleAnimation()],
})
export class LanguageTextsComponent extends AppComponentBase implements AfterViewInit, OnInit {
  @ViewChild('targetLanguageNameCombobox', { static: true }) targetLanguageNameCombobox: ElementRef;
  @ViewChild('baseLanguageNameCombobox', { static: true }) baseLanguageNameCombobox: ElementRef;
  @ViewChild('sourceNameCombobox', { static: true }) sourceNameCombobox: ElementRef;
  @ViewChild('targetValueFilterCombobox', { static: true }) targetValueFilterCombobox: ElementRef;
  @ViewChild('textsTable', { static: true }) textsTable: ElementRef;
  @ViewChild('editTextModal', { static: true }) editTextModal: EditTextModalComponent;
  @ViewChild('dataTable', { static: true }) dataTable: Table;
  @ViewChild('paginator', { static: true }) paginator: Paginator;

  sourceNames: string[] = [];
  languages: eaf.localization.ILanguageInfo[] = [];
  targetLanguageName: string;
  sourceName: string;
  baseLanguageName: string;
  targetValueFilter: string;

  filters: {
    filterText: string;
  } = <any>{};

  constructor(
    injector: Injector,
    private readonly _languageService: LanguageServiceProxy,
    private readonly _router: Router,
    private readonly _activatedRoute: ActivatedRoute,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.sourceNames =
      eaf.localization.sources
        ?.filter(source => source.type === 'MultiTenantLocalizationSource')
        .map(value => value.name) || [];
    this.languages = eaf.localization.languages;
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.init();
    });
  }

  getLanguageTexts(event?: LazyLoadEvent) {
    if (!this.paginator || !this.dataTable || !this.sourceName) {
      return;
    }

    this.dataTableHelper.showLoadingIndicator();

    this._languageService
      .getLanguageTexts(
        this.baseLanguageName,
        this.filters.filterText,
        this.dataTableHelper.getMaxResultCount(this.paginator, event),
        this.dataTableHelper.getSkipCount(this.paginator, event),
        this.dataTableHelper.getSorting(this.dataTable),
        this.sourceName,
        this.targetLanguageName,
        this.targetValueFilter,
      )
      .pipe(finalize(() => this.dataTableHelper.hideLoadingIndicator()))
      .subscribe(result => {
        this.dataTableHelper.totalRecordsCount = result.totalCount;
        this.dataTableHelper.records = result.items;
        this.dataTableHelper.hideLoadingIndicator();
      });
  }

  init(): void {
    this._activatedRoute.params.subscribe((params: Params) => {
      this.baseLanguageName = params['baseLanguageName'] || eaf.localization.currentLanguage.name;
      this.targetLanguageName = params['name'];
      this.sourceName = params['sourceName'] || 'ProjectName';
      this.targetValueFilter = params['targetValueFilter'] || 'ALL';
      this.filters.filterText = params['filterText'] || '';

      this.reloadPage();
    });
  }

  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
  }

  applyFilters(): void {
    this._router.navigate([
      'app/admin/languages',
      this.targetLanguageName,
      'texts',
      {
        sourceName: this.sourceName,
        baseLanguageName: this.baseLanguageName,
        targetValueFilter: this.targetValueFilter,
        filterText: this.filters.filterText,
      },
    ]);

    if (this.paginator.getPage() !== 0) {
      this.paginator.changePage(0);
    }
  }

  truncateString(text): string {
    return eaf.utils.truncateStringWithPostfix(text, 32, '...');
  }

  refreshTextValueFromModal(): void {
    for (const record of this.dataTableHelper.records) {
      if (record.key === this.editTextModal.model.key) {
        record.targetValue = this.editTextModal.model.value;
        return;
      }
    }
  }
}
