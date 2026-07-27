import { Component, EventEmitter, Injector, Output, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { TenantListDto, TenantServiceProxy } from '@shared/service-proxies/service-proxies';
import {
  AssociateUserToTenantInput,
  RemoveUserTenantAssociationInput,
  SetDefaultTenantInput,
  UserTenantAssociationService,
  UserTenantMembershipDto,
} from './user-tenant-association.service';

@Component({
  standalone: false,
  selector: 'userTenantMembershipModal',
  templateUrl: './user-tenant-membership-modal.component.html',
})
export class UserTenantMembershipModalComponent extends AppComponentBase {
  @ViewChild('userTenantMembershipModal', { static: true }) modal: ModalDirective;
  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

  userId: number | undefined;
  userName: string | undefined;
  active = false;
  saving = false;
  busy = false;

  memberships: UserTenantMembershipDto[] = [];
  tenants: TenantListDto[] = [];
  selectedTenantId: number | undefined;

  constructor(
    injector: Injector,
    private readonly _userTenantAssociationService: UserTenantAssociationService,
    private readonly _tenantService: TenantServiceProxy,
  ) {
    super(injector);
  }

  show(userId: number, userName: string): void {
    this.userId = userId;
    this.userName = userName;
    this.active = true;
    this.selectedTenantId = undefined;
    this.loadMemberships();
    this.loadTenants();
    this.modal.show();
  }

  close(): void {
    this.active = false;
    this.modal.hide();
  }

  loadMemberships(): void {
    if (!this.userId) {
      return;
    }

    this.busy = true;
    this._userTenantAssociationService
      .getByUser(this.userId)
      .pipe(finalize(() => (this.busy = false)))
      .subscribe(result => {
        this.memberships = result;
      });
  }

  loadTenants(): void {
    this.busy = true;
    this._tenantService
      .getTenants(undefined, undefined, undefined, undefined)
      .pipe(finalize(() => (this.busy = false)))
      .subscribe(result => {
        this.tenants = result.items || [];
      });
  }

  associate(): void {
    if (!this.userId || !this.selectedTenantId) {
      return;
    }

    const input = new class implements AssociateUserToTenantInput {
      userId = 0;
      tenantId = 0;
      isDefault = false;
    }();
    input.userId = this.userId;
    input.tenantId = this.selectedTenantId;
    input.isDefault = this.memberships.length === 0;

    this.saving = true;
    this._userTenantAssociationService
      .associate(input)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe(() => {
        this.loadMemberships();
        this.modalSave.emit(null);
      });
  }

  remove(tenantId: number): void {
    if (!this.userId) {
      return;
    }

    this.message.confirm('', this.l('AreYouSure'), isConfirmed => {
      if (!isConfirmed) {
        return;
      }

      const input = new class implements RemoveUserTenantAssociationInput {
        userId = 0;
        tenantId = 0;
      }();
      input.userId = this.userId!;
      input.tenantId = tenantId;

      this._userTenantAssociationService.remove(input).subscribe(() => {
        this.loadMemberships();
        this.modalSave.emit(null);
      });
    });
  }

  setDefault(tenantId: number): void {
    if (!this.userId) {
      return;
    }

    const input = new class implements SetDefaultTenantInput {
      userId = 0;
      tenantId = 0;
    }();
    input.userId = this.userId;
    input.tenantId = tenantId;

    this._userTenantAssociationService.setDefault(input).subscribe(() => {
      this.loadMemberships();
      this.modalSave.emit(null);
    });
  }

  getTenantName(tenantId: number): string {
    const tenant = this.tenants.find(t => t.id === tenantId);
    return tenant ? tenant.name || tenant.tenancyName : `${tenantId}`;
  }
}
