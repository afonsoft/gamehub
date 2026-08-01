import { Component, Injector, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { CreateMassNotificationInput, EntityDtoOfInt64, ICreateMassNotificationInput, IMassNotificationDto, MassNotificationServiceProxy } from '@shared/service-proxies/service-proxies';
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
        targetUserIds: undefined,
        targetRoleIds: undefined,
        targetOrganizationUnitIds: undefined,
        sendToAllUsers: false,
        scheduledTime: undefined,
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
            .getAll(
                this.filters.filterText,
                this.filters.status,
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

    showCreateModal(): void {
        this.newMassNotification = {
            subject: '',
            message: '',
            severity: 0,
            targetUserIds: undefined,
            targetRoleIds: undefined,
            targetOrganizationUnitIds: undefined,
            sendToAllUsers: false,
            scheduledTime: undefined,
        };
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
            .create(new CreateMassNotificationInput(this.newMassNotification as any))
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
                this._massNotificationService.cancel(new EntityDtoOfInt64({ id: massNotification.id } as any)).subscribe(() => {
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
