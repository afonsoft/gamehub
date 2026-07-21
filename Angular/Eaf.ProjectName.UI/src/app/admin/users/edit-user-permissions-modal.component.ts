import { Component, Injector, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { EntityDtoOfInt64, UpdateUserPermissionsInput, UserServiceProxy } from '@shared/service-proxies/service-proxies';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { PermissionTreeComponent } from '../shared/permission-tree.component';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  selector: 'editUserPermissionsModal',
  templateUrl: './edit-user-permissions-modal.component.html',
})
export class EditUserPermissionsModalComponent extends AppComponentBase {
  @ViewChild('editModal', { static: true }) modal: ModalDirective;
  @ViewChild('permissionTree', { static: true }) permissionTree: PermissionTreeComponent;

  saving = false;
  resettingPermissions = false;

  userId: number;
  userName: string;

  constructor(
    injector: Injector,
    private readonly _userService: UserServiceProxy,
  ) {
    super(injector);
  }

  show(userId: number, userName?: string): void {
    this.userId = userId;
    this.userName = userName;

    this._userService.getUserPermissionsForEdit(userId).subscribe(result => {
      this.permissionTree.editData = result;
      this.modal.show();
    });
  }

  save(): void {
    const input = new UpdateUserPermissionsInput();

    input.id = this.userId;
    input.grantedPermissionNames = this.permissionTree.getGrantedPermissionNames();

    this.saving = true;
    this._userService
      .updateUserPermissions(input)
      .pipe(
        finalize(() => {
          this.saving = false;
        }),
      )
      .subscribe(() => {
        this.notify.success(this.l('SavedSuccessfully'));
        this.close();
      });
  }

  resetPermissions(): void {
    const input = new EntityDtoOfInt64();

    input.id = this.userId;

    this.resettingPermissions = true;
    this._userService.resetUserSpecificPermissions(input).subscribe(
      () => {
        this.notify.info(this.l('ResetSuccessfully'));
        this._userService.getUserPermissionsForEdit(this.userId).subscribe(result => {
          this.permissionTree.editData = result;
        });
      },
      undefined,
      () => {
        this.resettingPermissions = false;
      },
    );
  }

  close(): void {
    this.modal.hide();
  }
}
