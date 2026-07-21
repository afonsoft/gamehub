import { Component, EventEmitter, Injector, Output, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { CreateOrUpdateRoleInput, RoleEditDto, RoleServiceProxy } from '@shared/service-proxies/service-proxies';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { PermissionTreeComponent } from '../shared/permission-tree.component';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  selector: 'createOrEditRoleModal',
  templateUrl: './create-or-edit-role-modal.component.html',
})
export class CreateOrEditRoleModalComponent extends AppComponentBase {
  @ViewChild('createOrEditModal', { static: true }) modal: ModalDirective;
  @ViewChild('permissionTree', { static: true }) permissionTree: PermissionTreeComponent;

  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

  active = false;
  saving = false;

  role: RoleEditDto = new RoleEditDto();
  constructor(
    injector: Injector,
    private readonly _roleService: RoleServiceProxy,
  ) {
    super(injector);
  }

  show(roleId?: number): void {

    this.active = true;

    this._roleService.getRoleForEdit(roleId).subscribe(result => {
      this.role = result.role;
      this.permissionTree.editData = result;
      this.modal.show();
    });
  }

  onShown(): void {
    document.getElementById('RoleDisplayName').focus();
  }

  save(): void {


    const input = new CreateOrUpdateRoleInput();
    input.role = this.role;
    input.grantedPermissionNames = this.permissionTree.getGrantedPermissionNames();

    this.saving = true;
    this._roleService
      .createOrUpdateRole(input)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe(() => {
        this.notify.success(this.l('SavedSuccessfully'));
        this.close();
        this.modalSave.emit(null);
      });
  }

  close(): void {
    this.active = false;
    this.modal.hide();
  }
}
