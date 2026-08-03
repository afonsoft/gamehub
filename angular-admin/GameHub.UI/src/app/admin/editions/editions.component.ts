import { Component, Injector, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { EditionServiceProxy } from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';
import { CreateOrEditEditionModalComponent } from './create-or-edit-edition-modal.component';
import { EditionFeaturesModalComponent } from './edition-features-modal.component';

@Component({
  standalone: false,
  templateUrl: './editions.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class EditionsComponent extends AppComponentBase implements OnInit {
  @ViewChild('dataTable', { static: true }) dataTable: Table;
  @ViewChild('paginator', { static: true }) paginator: Paginator;
  @ViewChild('createOrEditEditionModal', { static: true }) createOrEditEditionModal: CreateOrEditEditionModalComponent;
  @ViewChild('editionFeaturesModal', { static: true }) editionFeaturesModal: EditionFeaturesModalComponent;

  filters: { filterText: string } = { filterText: '' };

  constructor(
    injector: Injector,
    private readonly _editionService: EditionServiceProxy,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.filters.filterText = '';
  }

  getEditions(event?: LazyLoadEvent): void {
    if (this.dataTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);
      return;
    }

    this.dataTableHelper.showLoadingIndicator();

    this._editionService
      .getEditions(
        this.filters.filterText,
        this.dataTableHelper.getSorting(this.dataTable),
        this.dataTableHelper.getSkipCount(this.paginator, event),
        this.dataTableHelper.getMaxResultCount(this.paginator, event),
      )
      .pipe(finalize(() => this.dataTableHelper.hideLoadingIndicator()))
      .subscribe(result => {
        this.dataTableHelper.totalRecordsCount = result.totalCount;
        this.dataTableHelper.records = result.items;
      });
  }

  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
  }

  createEdition(): void {
    this.createOrEditEditionModal.show();
  }

  editEdition(record: any): void {
    this.createOrEditEditionModal.show(record.id);
  }

  deleteEdition(record: any): void {
    this.message.confirm(this.l('EditionDeleteWarningMessage', record.displayName), this.l('AreYouSure'), isConfirmed => {
      if (isConfirmed) {
        this._editionService.deleteEdition(record.id).subscribe(() => {
          this.notify.success(this.l('SuccessfullyDeleted'));
          this.reloadPage();
        });
      }
    });
  }

  showFeatures(record: any): void {
    this.editionFeaturesModal.show(record.id, record.displayName);
  }
}
