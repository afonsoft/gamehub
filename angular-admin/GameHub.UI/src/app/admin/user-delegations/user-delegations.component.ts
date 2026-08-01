import { Component, Injector, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ICreateUserDelegationInput, IUserDelegationDto, UserDelegationServiceProxy } from '@shared/service-proxies/user-delegation.service-proxy';
import { UserListDto, UserServiceProxy } from '@shared/service-proxies/service-proxies';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
    standalone: false,
    templateUrl: './user-delegations.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()],
})
export class UserDelegationsComponent extends AppComponentBase implements OnInit {
    @ViewChild('createModal', { static: true }) modal: ModalDirective;

    myDelegations: IUserDelegationDto[] = [];
    delegatedUsers: IUserDelegationDto[] = [];
    loading = false;
    saving = false;

    activeTab: 'myDelegations' | 'delegatedUsers' = 'myDelegations';

    allUsers: UserListDto[] = [];

    newDelegation: ICreateUserDelegationInput & { targetUserId?: number } = {
        targetUserId: undefined,
        startTime: '',
        endTime: '',
        description: '',
    };

    constructor(
        injector: Injector,
        private readonly _userDelegationService: UserDelegationServiceProxy,
        private readonly _userService: UserServiceProxy,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.loadUsers();
        this.loadDelegations();
    }

    loadUsers(): void {
        this._userService.getUsers('', 'Name asc', 1000, 0).subscribe(result => {
            this.allUsers = result.items ?? [];
        });
    }

    loadDelegations(): void {
        this.loading = true;
        this._userDelegationService
            .getMyDelegations({ maxResultCount: 1000 })
            .pipe(finalize(() => (this.loading = false)))
            .subscribe(result => {
                this.myDelegations = result.items ?? [];
            });

        this._userDelegationService
            .getDelegatedUsers({ maxResultCount: 1000 })
            .pipe(finalize(() => (this.loading = false)))
            .subscribe(result => {
                this.delegatedUsers = result.items ?? [];
            });
    }

    showCreateModal(): void {
        const now = new Date();
        const defaultStart = new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
        const defaultEnd = new Date(now.getTime() + 24 * 60 * 60 * 1000 - now.getTimezoneOffset() * 60000).toISOString().slice(0, 16);

        this.newDelegation = {
            targetUserId: undefined,
            startTime: defaultStart,
            endTime: defaultEnd,
            description: '',
        };
        this.modal.show();
    }

    closeModal(): void {
        this.modal.hide();
    }

    save(): void {
        if (!this.newDelegation.targetUserId) {
            this.notify.warn(this.l('ThisFieldIsRequired'));
            return;
        }

        if (!this.newDelegation.startTime || !this.newDelegation.endTime) {
            this.notify.warn(this.l('ThisFieldIsRequired'));
            return;
        }

        if (new Date(this.newDelegation.startTime) >= new Date(this.newDelegation.endTime)) {
            this.notify.warn(this.l('StartTimeMustBeLessThanEndTime'));
            return;
        }

        this.saving = true;
        const input: ICreateUserDelegationInput = {
            targetUserId: this.newDelegation.targetUserId,
            startTime: this.newDelegation.startTime,
            endTime: this.newDelegation.endTime,
            description: this.newDelegation.description,
        };

        this._userDelegationService
            .create(input)
            .pipe(finalize(() => (this.saving = false)))
            .subscribe(() => {
                this.notify.success(this.l('SavedSuccessfully'));
                this.closeModal();
                this.loadDelegations();
            });
    }

    cancel(delegation: IUserDelegationDto): void {
        this.message.confirm(this.l('AreYouSure'), this.l('UserDelegationCancelWarningMessage', delegation.targetUserName), isConfirmed => {
            if (isConfirmed) {
                this._userDelegationService.cancel(delegation.id).subscribe(() => {
                    this.notify.success(this.l('SuccessfullyDeleted'));
                    this.loadDelegations();
                });
            }
        });
    }
}
