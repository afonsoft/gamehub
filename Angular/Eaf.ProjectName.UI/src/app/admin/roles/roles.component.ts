import { Component, Injector, ViewChild, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { RoleListDto, RoleServiceProxy } from '@shared/service-proxies/service-proxies';
import { Table } from 'primeng/table';
import { CreateOrEditRoleModalComponent } from './create-or-edit-role-modal.component';
import { EntityTypeHistoryModalComponent } from '@app/shared/common/entityHistory/entity-type-history-modal.component';
import * as _ from 'lodash';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  templateUrl: './roles.component.html',
  animations: [appModuleAnimation()],
})
export class RolesComponent extends AppComponentBase implements OnInit {
  @ViewChild('createOrEditRoleModal', { static: true }) createOrEditRoleModal: CreateOrEditRoleModalComponent;
  @ViewChild('entityTypeHistoryModal', { static: true }) entityTypeHistoryModal: EntityTypeHistoryModalComponent;
  @ViewChild('dataTable', { static: true }) dataTable: Table;

  _entityTypeFullName = 'Eaf.Middleware.Authorization.Roles.Role';
  entityHistoryEnabled = false;

  filters: {
    filterText: string;
  } = <any>{};

  constructor(
    injector: Injector,
    private readonly _roleService: RoleServiceProxy,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.filters.filterText = '';
    this.setIsEntityHistoryEnabled();
  }

  private setIsEntityHistoryEnabled(): void {
    const customSettings = (eaf as any).custom;
    this.entityHistoryEnabled =
      customSettings.EntityHistory?.isEnabled &&
      _.filter(customSettings.EntityHistory.enabledEntities, entityType => entityType === this._entityTypeFullName).length === 1;
  }

  getRoles(): void {
    this.dataTableHelper.showLoadingIndicator();
    const permission = this.filters.filterText ? this.filters.filterText : undefined;

    this._roleService
      .getRoles(permission, this.dataTableHelper.getSorting(this.dataTable))
      .pipe(finalize(() => this.dataTableHelper.hideLoadingIndicator()))
      .subscribe(result => {
        this.dataTableHelper.records = result.items;
        this.dataTableHelper.totalRecordsCount = result.items.length;
        this.dataTableHelper.hideLoadingIndicator();
      });
  }

  createRole(): void {
    this.createOrEditRoleModal.show();
  }

  showHistory(role: RoleListDto): void {
    this.entityTypeHistoryModal.show({
      entityId: role.id.toString(),
      entityTypeFullName: this._entityTypeFullName,
      entityTypeDescription: role.displayName,
    });
  }

  deleteRole(role: RoleListDto): void {

    this.message.confirm(this.l('RoleDeleteWarningMessage', role.displayName), this.l('AreYouSure'), isConfirmed => {
      if (isConfirmed) {
        this._roleService.deleteRole(role.id).subscribe(() => {
          this.getRoles();
          eaf.notify.success(this.l('SuccessfullyDeleted'));
        });
      }
    });
  }
}
