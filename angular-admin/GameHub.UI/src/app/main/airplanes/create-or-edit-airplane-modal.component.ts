import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { AirplanesServiceProxy, CreateOrEditAirplaneDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
  standalone: false,
  selector: 'createOrEditAirplaneModal',
  templateUrl: './create-or-edit-airplane-modal.component.html',
})
export class CreateOrEditAirplaneModalComponent extends AppComponentBase {
  @ViewChild('createOrEditModal', { static: true }) modal: ModalDirective;
  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

  active = false;
  saving = false;

  airplane: CreateOrEditAirplaneDto = new CreateOrEditAirplaneDto();

  constructor(
    injector: Injector,
    private readonly _airplanesServiceProxy: AirplanesServiceProxy,
  ) {
    super(injector);
  }

  show(airplaneId?: number): void {
    if (!airplaneId) {
      this.airplane = new CreateOrEditAirplaneDto();
      this.active = true;
      this.modal.show();
    } else {
      this._airplanesServiceProxy.getAirplaneForEdit(airplaneId).subscribe(result => {
        this.airplane = result;
        this.active = true;
        this.modal.show();
      });
    }
  }

  onShown(): void {
    document.getElementById('Number').focus();
  }

  save(): void {
    this.saving = true;
    this._airplanesServiceProxy
      .createOrEdit(this.airplane)
      .pipe(
        finalize(() => {
          this.saving = false;
        }),
      )
      .subscribe(() => {
        this.notify.info(this.l('SavedSuccessfully'));
        this.close();
        this.modalSave.emit(null);
      });
  }

  close(): void {
    this.active = false;
    this.modal.hide();
  }
}
