import { Component, ElementRef, Injector, ViewChild, ViewEncapsulation, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ApplicationLanguageListDto, LanguageServiceProxy, SetDefaultLanguageInput } from '@shared/service-proxies/service-proxies';
import { Table } from 'primeng/table';
import { CreateOrEditLanguageModalComponent } from './create-or-edit-language-modal.component';
import { EafSessionService } from '@eaf/session/eaf-session.service';
import { finalize } from 'rxjs/operators';


@Component({
  standalone: false,
  templateUrl: './languages.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class LanguagesComponent extends AppComponentBase implements OnInit {
  @ViewChild('languagesTable', { static: true }) languagesTable: ElementRef;
  @ViewChild('createOrEditLanguageModal', { static: true }) createOrEditLanguageModal: CreateOrEditLanguageModalComponent;
  @ViewChild('dataTable', { static: true }) dataTable: Table;

  defaultLanguageName: string;

  filters: {
    filterText: string;
  } = <any>{};

  get multiTenancySideIsHost(): boolean {
    return !this._sessionService.tenantId;
  }

  constructor(
    injector: Injector,
    private readonly _activatedRoute: ActivatedRoute,
    private readonly _languageService: LanguageServiceProxy,
    private readonly _sessionService: EafSessionService,
    private readonly _router: Router,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.filters.filterText = this._activatedRoute.snapshot.queryParams['filterText'] || '';
  }

  getLanguages(): void {
    this.dataTableHelper.showLoadingIndicator();

    this._languageService
      .getLanguages(this.filters.filterText, this.dataTableHelper.getSorting(this.dataTable))
      .pipe(finalize(() => this.dataTableHelper.hideLoadingIndicator()))
      .subscribe(result => {
        this.defaultLanguageName = result.defaultLanguageName;
        this.dataTableHelper.records = result.items;
        this.dataTableHelper.totalRecordsCount = result.items.length;
        this.dataTableHelper.hideLoadingIndicator();
      });
  }

  changeTexts(language: ApplicationLanguageListDto): void {
    this._router.navigate(['app/admin/languages', language.name, 'texts']);
  }

  setAsDefaultLanguage(language: ApplicationLanguageListDto): void {
    const input = new SetDefaultLanguageInput();
    input.name = language.name;
    this._languageService.setDefaultLanguage(input).subscribe(() => {
      this.getLanguages();
      this.notify.success(this.l('SuccessfullySaved'));
    });
  }

  deleteLanguage(language: ApplicationLanguageListDto): void {
    this.message.confirm(this.l('LanguageDeleteWarningMessage', language.displayName), this.l('AreYouSure'), isConfirmed => {
      if (isConfirmed) {
        this._languageService.deleteLanguage(language.id).subscribe(() => {
          this._languageService.getAllLanguages().subscribe(result => {
            eaf.localization.languages = result;
            eaf.event.trigger('app.languagesChanged');
            this.getLanguages();
            this.notify.success(this.l('SuccessfullyDeleted'));
          });
        });
      }
    });
  }
}
