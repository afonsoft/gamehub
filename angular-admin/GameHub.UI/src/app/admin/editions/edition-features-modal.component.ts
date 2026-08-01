import { Component, EventEmitter, Injector, Output, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { EditionServiceProxy, INameValueDto, IUpdateEditionFeaturesInput, NameValueDto, UpdateEditionFeaturesInput } from '@shared/service-proxies/service-proxies';
import { FeatureTreeEditModel } from '@app/admin/shared/feature-tree-edit.model';
import { FeatureTreeComponent } from '@app/admin/shared/feature-tree.component';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
    standalone: false,
    selector: 'editionFeaturesModal',
    templateUrl: './edition-features-modal.component.html',
    animations: [appModuleAnimation()],
})
export class EditionFeaturesModalComponent extends AppComponentBase {
    @ViewChild('editionFeaturesModal', { static: true }) modal: ModalDirective;
    @ViewChild('featureTree', { static: true }) featureTree: FeatureTreeComponent;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;
    editionId: number;
    editionName: string;

    constructor(
        injector: Injector,
        private readonly _editionService: EditionServiceProxy,
    ) {
        super(injector);
    }

    show(editionId: number, editionName: string): void {
        this.active = true;
        this.editionId = editionId;
        this.editionName = editionName;
        this.modal.show();
        this.loadFeatures();
    }

    loadFeatures(): void {
        this._editionService.getEditionFeaturesForEdit(this.editionId).subscribe(result => {
            this.featureTree.editData = result as FeatureTreeEditModel;
        });
    }

    save(): void {
        if (!this.featureTree.areAllValuesValid()) {
            this.message.warn(this.l('InvalidFeaturesWarning'));
            return;
        }

        const featureValues = this.featureTree.getGrantedFeatures().map(f => new NameValueDto({ name: f.name, value: f.value } as any));
        const input = new UpdateEditionFeaturesInput({
            id: this.editionId,
            featureValues,
        } as any);

        this.saving = true;
        this._editionService
            .updateEditionFeatures(input)
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
