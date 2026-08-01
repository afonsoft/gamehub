import { Component, Injector, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { CreateOrganizationUnitInput, IOrganizationUnitDto, OrganizationUnitServiceProxy, RoleToOrganizationUnitInput, UpdateOrganizationUnitInput, UserToOrganizationUnitInput } from '@shared/service-proxies/service-proxies';
import { RoleServiceProxy, RoleListDto, UserServiceProxy, UserListDto } from '@shared/service-proxies/service-proxies';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

interface IFlatOrganizationUnit extends IOrganizationUnitDto {
    level: number;
}

@Component({
    standalone: false,
    templateUrl: './organization-units.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()],
})
export class OrganizationUnitsComponent extends AppComponentBase implements OnInit {
    @ViewChild('createOrEditModal', { static: true }) modal: ModalDirective;
    @ViewChild('membersModal', { static: true }) membersModal: ModalDirective;
    @ViewChild('rolesModal', { static: true }) rolesModal: ModalDirective;

    organizationUnits: IOrganizationUnitDto[] = [];
    flatOrganizationUnits: IFlatOrganizationUnit[] = [];
    loading = false;
    saving = false;

    activeOu: IOrganizationUnitDto = { id: 0, displayName: '', code: '', parentId: undefined, children: [] };
    isEdit = false;

    // Members management
    membersLoading = false;
    membersSaving = false;
    selectedMemberUserId: number | undefined;
    organizationUnitUsers: any[] = [];
    allUsers: UserListDto[] = [];
    memberFilter = '';

    // Roles management
    rolesLoading = false;
    rolesSaving = false;
    selectedRoleId: number | undefined;
    organizationUnitRoles: any[] = [];
    allRoles: RoleListDto[] = [];
    roleFilter = '';

    constructor(
        injector: Injector,
        private readonly _organizationUnitService: OrganizationUnitServiceProxy,
        private readonly _userService: UserServiceProxy,
        private readonly _roleService: RoleServiceProxy,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.loadOrganizationUnits();
    }

    loadOrganizationUnits(): void {
        this.loading = true;
        this._organizationUnitService
            .getOrganizationUnits()
            .pipe(finalize(() => (this.loading = false)))
            .subscribe(result => {
                this.organizationUnits = result.items ?? [];
                this.flattenOrganizationUnits();
            });
    }

    flattenOrganizationUnits(): void {
        this.flatOrganizationUnits = [];
        const walk = (items: IOrganizationUnitDto[], level: number) => {
            if (!items) return;
            for (const item of items) {
                this.flatOrganizationUnits.push({ ...item, level });
                walk(item.children, level + 1);
            }
        };
        walk(this.organizationUnits, 0);
    }

    showCreateModal(parentId?: number): void {
        this.isEdit = false;
        this.activeOu = { id: 0, displayName: '', code: '', parentId: parentId ?? undefined, children: [] };
        this.modal.show();
    }

    showEditModal(ou: IOrganizationUnitDto): void {
        this.isEdit = true;
        this.activeOu = { ...ou, parentId: ou.parentId ?? undefined, children: ou.children ?? [] };
        this.modal.show();
    }

    save(): void {
        if (!this.activeOu.displayName) {
            this.notify.warn(this.l('RequiredField', this.l('Name')));
            return;
        }

        this.saving = true;
        const request = this.isEdit
            ? this._organizationUnitService.update(new UpdateOrganizationUnitInput({ id: this.activeOu.id, displayName: this.activeOu.displayName } as any))
            : this._organizationUnitService.create(new CreateOrganizationUnitInput({ displayName: this.activeOu.displayName, parentId: this.activeOu.parentId } as any));

        request.pipe(finalize(() => (this.saving = false))).subscribe(() => {
            this.notify.success(this.l('SavedSuccessfully'));
            this.closeModal();
            this.loadOrganizationUnits();
        });
    }

    deleteOu(ou: IOrganizationUnitDto): void {
        this.message.confirm('', this.l('OrganizationUnitDeleteWarningMessage', ou.displayName), isConfirmed => {
            if (isConfirmed) {
                this._organizationUnitService.delete(ou.id).subscribe(() => {
                    this.notify.success(this.l('SuccessfullyDeleted'));
                    this.loadOrganizationUnits();
                });
            }
        });
    }

    closeModal(): void {
        this.modal.hide();
    }

    indent(level: number): { 'padding-left': string } {
        return { 'padding-left': level * 24 + 'px' };
    }

    // Members management
    showMembersModal(ou: IOrganizationUnitDto): void {
        this.activeOu = { ...ou };
        this.selectedMemberUserId = undefined;
        this.memberFilter = '';
        this.organizationUnitUsers = [];
        this.loadAllUsers();
        this.loadMembers();
        this.membersModal.show();
    }

    loadAllUsers(): void {
        this._userService.getUsers('', 'Name asc', 100, 0).subscribe(result => {
            this.allUsers = result.items ?? [];
        });
    }

    loadMembers(): void {
        this.membersLoading = true;
        this._organizationUnitService
            .getOrganizationUnitUsers(this.activeOu.id, this.memberFilter, 'Name asc', 0, 100)
            .pipe(finalize(() => (this.membersLoading = false)))
            .subscribe(result => {
                this.organizationUnitUsers = result.items ?? [];
            });
    }

    addMember(): void {
        if (!this.selectedMemberUserId) {
            this.notify.warn(this.l('RequiredField', this.l('UserName')));
            return;
        }

        this.membersSaving = true;
        this._organizationUnitService
            .addUserToOrganizationUnit(new UserToOrganizationUnitInput({ organizationUnitId: this.activeOu.id, userId: this.selectedMemberUserId } as any))
            .pipe(finalize(() => (this.membersSaving = false)))
            .subscribe(() => {
                this.notify.success(this.l('SavedSuccessfully'));
                this.selectedMemberUserId = undefined;
                this.loadMembers();
            });
    }

    removeMember(user: any): void {
        this.message.confirm('', this.l('OrganizationUnitUserRemoveWarningMessage', user.userName), isConfirmed => {
            if (isConfirmed) {
                this._organizationUnitService
                    .removeUserFromOrganizationUnit(this.activeOu.id, user.userId)
                    .subscribe(() => {
                        this.notify.success(this.l('SuccessfullyDeleted'));
                        this.loadMembers();
                    });
            }
        });
    }

    closeMembersModal(): void {
        this.membersModal.hide();
    }

    // Roles management
    showRolesModal(ou: IOrganizationUnitDto): void {
        this.activeOu = { ...ou };
        this.selectedRoleId = undefined;
        this.roleFilter = '';
        this.organizationUnitRoles = [];
        this.loadAllRoles();
        this.loadRoles();
        this.rolesModal.show();
    }

    loadAllRoles(): void {
        this._roleService.getRoles('', 'Name asc').subscribe(result => {
            this.allRoles = result.items ?? [];
        });
    }

    loadRoles(): void {
        this.rolesLoading = true;
        this._organizationUnitService
            .getOrganizationUnitRoles(this.activeOu.id, this.roleFilter, 'Name asc', 0, 100)
            .pipe(finalize(() => (this.rolesLoading = false)))
            .subscribe(result => {
                this.organizationUnitRoles = result.items ?? [];
            });
    }

    addRole(): void {
        if (!this.selectedRoleId) {
            this.notify.warn(this.l('RequiredField', this.l('RoleName')));
            return;
        }

        this.rolesSaving = true;
        this._organizationUnitService
            .addRoleToOrganizationUnit(new RoleToOrganizationUnitInput({ organizationUnitId: this.activeOu.id, roleId: this.selectedRoleId } as any))
            .pipe(finalize(() => (this.rolesSaving = false)))
            .subscribe(() => {
                this.notify.success(this.l('SavedSuccessfully'));
                this.selectedRoleId = undefined;
                this.loadRoles();
            });
    }

    removeRole(role: any): void {
        this.message.confirm('', this.l('OrganizationUnitRoleRemoveWarningMessage', role.roleName), isConfirmed => {
            if (isConfirmed) {
                this._organizationUnitService
                    .removeRoleFromOrganizationUnit(this.activeOu.id, role.roleId)
                    .subscribe(() => {
                        this.notify.success(this.l('SuccessfullyDeleted'));
                        this.loadRoles();
                    });
            }
        });
    }

    closeRolesModal(): void {
        this.rolesModal.hide();
    }
}
