import { Component, Injector, ViewChild, ViewEncapsulation, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppConsts } from '@shared/AppConsts';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { EntityDtoOfInt64, UserListDto, UserServiceProxy } from '@shared/service-proxies/service-proxies';
import { FileDownloadService } from '@shared/utils/file-download.service';
import { LazyLoadEvent } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { CreateOrEditUserModalComponent } from './create-or-edit-user-modal.component';
import { EditUserPermissionsModalComponent } from './edit-user-permissions-modal.component';
import { ImpersonationService } from './impersonation.service';
import { UserTenantMembershipModalComponent } from './user-tenant-membership-modal.component';
import { finalize } from 'rxjs/operators';
import { EntityTypeHistoryModalComponent } from '@app/shared/common/entityHistory/entity-type-history-modal.component';
import * as _ from 'lodash';

@Component({
  standalone: false,
  templateUrl: './users.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class UsersComponent extends AppComponentBase implements OnInit {
  @ViewChild('createOrEditUserModal', { static: true }) createOrEditUserModal: CreateOrEditUserModalComponent;
  @ViewChild('editUserPermissionsModal', { static: true }) editUserPermissionsModal: EditUserPermissionsModalComponent;
  @ViewChild('userTenantMembershipModal', { static: true }) userTenantMembershipModal: UserTenantMembershipModalComponent;
  @ViewChild('entityTypeHistoryModal', { static: true }) entityTypeHistoryModal: EntityTypeHistoryModalComponent;
  @ViewChild('dataTable', { static: true }) dataTable: Table;
  @ViewChild('paginator', { static: true }) paginator: Paginator;

  _entityTypeFullName = 'Eaf.Middleware.Authorization.Users.User';
  entityHistoryEnabled = false;

  filters: {
    filterText: string;
  } = <any>{};

  constructor(
    injector: Injector,
    public _impersonationService: ImpersonationService,
    private readonly _userServiceProxy: UserServiceProxy,
    private readonly _fileDownloadService: FileDownloadService,
    private readonly _activatedRoute: ActivatedRoute,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.filters.filterText = this._activatedRoute.snapshot.queryParams['filterText'] || '';
    this.setIsEntityHistoryEnabled();
  }

  private setIsEntityHistoryEnabled(): void {
    const customSettings = (eaf as any).custom;
    this.entityHistoryEnabled =
      customSettings.EntityHistory?.isEnabled &&
      _.filter(customSettings.EntityHistory.enabledEntities, entityType => entityType === this._entityTypeFullName).length === 1;
  }

  getUsers(event?: LazyLoadEvent) {
    if (this.dataTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);
      return;
    }

    this.dataTableHelper.showLoadingIndicator();

    this._userServiceProxy
      .getUsers(
        this.filters.filterText,
        this.dataTableHelper.getSorting(this.dataTable),
        this.dataTableHelper.getMaxResultCount(this.paginator, event),
        this.dataTableHelper.getSkipCount(this.paginator, event),
      )
      .pipe(finalize(() => this.dataTableHelper.hideLoadingIndicator()))
      .subscribe(result => {
        this.dataTableHelper.totalRecordsCount = result.totalCount;
        this.dataTableHelper.records = result.items;
        this.dataTableHelper.hideLoadingIndicator();
      });
  }

  unlockUser(record): void {
    this._userServiceProxy.unlockUser(new EntityDtoOfInt64({ id: record.id })).subscribe(() => {
      this.notify.success(this.l('UnlockedTheUser', record.userName));
    });
  }

  closeSessionUser(record): void {
    this._userServiceProxy.closeSessionUser(record.id).subscribe(() => {
      this.notify.success(this.l('CloseSessionTheUser', record.userName));
    });
  }

  showHistory(user: UserListDto): void {
    this.entityTypeHistoryModal.show({
      entityId: user.id.toString(),
      entityTypeFullName: this._entityTypeFullName,
      entityTypeDescription: user.userName,
    });
  }

  getRolesAsString(roles): string {
    let roleNames = '';

    for (const role of roles) {
      if (roleNames.length) {
        roleNames = roleNames + ', ';
      }

      roleNames = roleNames + role.roleName;
    }

    return roleNames;
  }

  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
  }

  showTenantMembership(user: UserListDto): void {
    this.userTenantMembershipModal.show(user.id, user.userName);
  }

  exportToExcel(): void {
    this._userServiceProxy.getUsersToExcel().subscribe(result => {
      this._fileDownloadService.downloadTempFile(result);
    });
  }

  createUser(): void {
    this.createOrEditUserModal.show();
  }

  deleteUser(user: UserListDto): void {
    if (user.userName === AppConsts.userManagement.defaultAdminUserName) {
      this.message.warn(this.l('{0}UserCannotBeDeleted', AppConsts.userManagement.defaultAdminUserName));
      return;
    }

    this.message.confirm(this.l('UserDeleteWarningMessage', user.userName), this.l('AreYouSure'), isConfirmed => {
      if (isConfirmed) {
        this._userServiceProxy.deleteUser(user.id).subscribe(() => {
          this.reloadPage();
          this.notify.success(this.l('SuccessfullyDeleted'));
        });
      }
    });
  }
}
