import { Component, Injector, Input, OnChanges, SimpleChanges, ViewEncapsulation } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { finalize } from 'rxjs/operators';
import {
  CreateDynamicEntityPropertyInput,
  CreateOrUpdateDynamicEntityPropertyValueInput,
  DynamicEntityPropertyDto,
  DynamicEntityPropertyService,
  DynamicEntityPropertyValueDto,
  GetDynamicEntityPropertyValuesInput,
} from '@shared/service-proxies/dynamic-entity-property.service';

@Component({
  selector: 'app-dynamic-entity-property-manager',
  standalone: false,
  templateUrl: './dynamic-entity-property-manager.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class DynamicEntityPropertyManagerComponent extends AppComponentBase implements OnChanges {
  @Input() entityFullName: string | undefined;
  @Input() entityId: string | undefined;
  @Input() dynamicPropertyId: number | undefined;

  loading = false;
  dynamicEntityProperty: DynamicEntityPropertyDto | undefined;
  currentValue: DynamicEntityPropertyValueDto | undefined;
  valueInput: string | undefined;

  constructor(
    injector: Injector,
    private readonly _dynamicEntityPropertyService: DynamicEntityPropertyService,
  ) {
    super(injector);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.entityFullName && this.entityId && this.dynamicPropertyId !== undefined && this.dynamicPropertyId !== null) {
      this.load();
    }
  }

  load(): void {
    if (!this.entityFullName || !this.entityId || this.dynamicPropertyId === undefined || this.dynamicPropertyId === null) {
      return;
    }

    this.loading = true;
    this._dynamicEntityPropertyService
      .getAllDynamicEntityProperties(this.entityFullName)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        const mapping = result.items?.find(x => x.dynamicPropertyId === this.dynamicPropertyId);
        if (mapping) {
          this.dynamicEntityProperty = mapping;
          this.loadValue();
        } else {
          this.createMapping();
        }
      });
  }

  private createMapping(): void {
    if (!this.entityFullName || !this.dynamicPropertyId) {
      return;
    }

    const input = new CreateDynamicEntityPropertyInput();
    input.entityFullName = this.entityFullName;
    input.dynamicPropertyId = this.dynamicPropertyId;

    this.loading = true;
    this._dynamicEntityPropertyService
      .createDynamicEntityProperty(input)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.dynamicEntityProperty = result;
        this.loadValue();
      });
  }

  private loadValue(): void {
    if (!this.dynamicEntityProperty?.id || !this.entityId) {
      return;
    }

    const input = new GetDynamicEntityPropertyValuesInput();
    input.entityFullName = this.entityFullName;
    input.entityId = this.entityId;
    input.dynamicEntityPropertyId = this.dynamicEntityProperty.id;

    this.loading = true;
    this._dynamicEntityPropertyService
      .getAllDynamicEntityPropertyValues(input)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.currentValue = result.items?.length ? result.items[0] : undefined;
        this.valueInput = this.currentValue?.value;
      });
  }

  save(): void {
    if (!this.dynamicEntityProperty?.id || !this.entityId) {
      return;
    }

    const input = new CreateOrUpdateDynamicEntityPropertyValueInput();
    input.id = this.currentValue?.id;
    input.entityId = this.entityId;
    input.dynamicEntityPropertyId = this.dynamicEntityProperty.id;
    input.value = this.valueInput ?? '';

    const operation = input.id
      ? this._dynamicEntityPropertyService.updateDynamicEntityPropertyValue(input)
      : this._dynamicEntityPropertyService.createDynamicEntityPropertyValue(input);

    this.loading = true;
    operation.pipe(finalize(() => (this.loading = false))).subscribe(() => {
      this.notify.success(this.l('SavedSuccessfully'));
      this.loadValue();
    });
  }
}
