import { Component, Injector, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ICreateMassNotificationInput, IMassNotificationDto, MassNotificationServiceProxy } from '@shared/service-proxies/mass-notification.service-proxy';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { LazyLoadEvent } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';

@Component({
    standalone: false,
    templateUrl: './mass-notifications.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()],
})
export class MassNotificationsComponent extends AppComponentBase implements OnInit {
    @ViewChild('dataTable', { static: true }) dataTable: Table;
    @ViewChild('paginator', { static: true }) paginator: Paginator;
    @ViewChild('createModal', { static: true }) modal: ModalDirective;

    filters: { filterText: string; status: string } = { filterText: '', status: '' };
    massNotifications: IMassNotificationDto[] = [];
    saving = false;

    newMassNotification: ICreateMassNotificationInput = {
        subject: '',
        message: '',
        severity: 0,
        sendToAllUsers: false,
    };

    constructor(
        injector: Injector,
        private readonly _massNotificationService: MassNotificationServiceProxy,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.resetFilters();
    }

    resetFilters(): void {
        this.filters = { filterText: '', status: '' };
    }

    getMassNotifications(event?: LazyLoadEvent): void {
        if (this.dataTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.dataTableHelper.showLoadingIndicator();
        this._massNotificationService
            .getAll({
                filter: this.filters.filterText,
                status: this.filters.status,
                sorting: this.dataTableHelper.getSorting(this.dataTable),
                skipCount: this.dataTableHelper.getSkipCount(this.paginator, event),
                maxResultCount: this.dataTableHelper.getMaxResultCount(this.paginator, event),
            })
            .pipe(finalize(() => this.dataTableHelper.hideLoadingIndicator()))
            .subscribe(result => {
                this.dataTableHelper.totalRecordsCount = result.totalCount;
                this.dataTableHelper.records = result.items;
            });
    }

    showCreateModal(): void {
        this.newMassNotification = { subject: '', message: '', severity: 0, sendToAllUsers: false };
        this.modal.show();
    }

    closeModal(): void {
        this.modal.hide();
    }

    save(): void {
        if (!this.newMassNotification.subject || !this.newMassNotification.message) {
            this.notify.warn(this.l('ThisFieldIsRequired'));
            return;
        }

        this.saving = true;
        this._massNotificationService
            .create(this.newMassNotification)
            .pipe(finalize(() => (this.saving = false)))
            .subscribe(() => {
                this.notify.success(this.l('SavedSuccessfully'));
                this.closeModal();
                this.getMassNotifications();
            });
    }

    cancel(massNotification: IMassNotificationDto): void {
        this.message.confirm(this.l('AreYouSure'), this.l('CancelingMassNotification'), isConfirmed => {
            if (isConfirmed) {
                this._massNotificationService.cancel(massNotification.id).subscribe(() => {
                    this.notify.success(this.l('SuccessfullyDeleted'));
                    this.getMassNotifications();
                });
            }
        });
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage());
    }
}
