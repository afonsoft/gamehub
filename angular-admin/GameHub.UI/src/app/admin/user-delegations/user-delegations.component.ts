import { Component, Injector, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { CreateUserDelegationInput, EntityDtoOfInt64, IUserDelegationDto, UserDelegationServiceProxy, UserListDto, UserServiceProxy } from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';
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

    newDelegation: { targetUserId?: number; startTime: string; endTime: string; description: string } = {
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
            .getMyDelegations(undefined, undefined, '', 0, 1000)
            .pipe(finalize(() => (this.loading = false)))
            .subscribe(result => {
                this.myDelegations = result.items ?? [];
            });

        this._userDelegationService
            .getDelegatedUsers(undefined, undefined, '', 0, 1000)
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
        this._userDelegationService
            .create(new CreateUserDelegationInput({
                targetUserId: this.newDelegation.targetUserId,
                startTime: moment(this.newDelegation.startTime),
                endTime: moment(this.newDelegation.endTime),
                description: this.newDelegation.description,
            } as any))
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
                this._userDelegationService.cancel(new EntityDtoOfInt64({ id: delegation.id } as any)).subscribe(() => {
                    this.notify.success(this.l('SuccessfullyDeleted'));
                    this.loadDelegations();
                });
            }
        });
    }
}
