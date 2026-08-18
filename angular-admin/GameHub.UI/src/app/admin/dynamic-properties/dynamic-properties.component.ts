import { Component, Injector, OnInit, ViewEncapsulation } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { finalize } from 'rxjs/operators';
import {
  CreateOrUpdateDynamicPropertyInput,
  DynamicEntityPropertyService,
  DynamicPropertyDto,
  DynamicPropertyValueDto,
} from '@shared/service-proxies/dynamic-entity-property.service';

@Component({
  standalone: false,
  templateUrl: './dynamic-properties.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class DynamicPropertiesComponent extends AppComponentBase implements OnInit {
  loading = false;
  saving = false;
  displayDialog = false;
  properties: DynamicPropertyDto[] = [];
  property: CreateOrUpdateDynamicPropertyInput = new CreateOrUpdateDynamicPropertyInput();
  valuesText = '';
  inputTypes: string[] = ['SingleLineStringInputType', 'ComboboxInputType', 'CheckboxInputType', 'MultiSelectComboboxInputType'];

  constructor(
    injector: Injector,
    private readonly _dynamicEntityPropertyService: DynamicEntityPropertyService,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.loadProperties();
  }

  loadProperties(): void {
    this.loading = true;
    this._dynamicEntityPropertyService
      .getAllDynamicProperties()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.properties = result.items ?? [];
      });
  }

  openNew(): void {
    this.property = new CreateOrUpdateDynamicPropertyInput();
    this.valuesText = '';
    this.displayDialog = true;
  }

  edit(item: DynamicPropertyDto): void {
    this.property = new CreateOrUpdateDynamicPropertyInput();
    this.property.id = item.id;
    this.property.propertyName = item.propertyName;
    this.property.displayName = item.displayName;
    this.property.inputType = item.inputType;
    this.property.permission = item.permission;
    this.valuesText = (item.values ?? []).map(v => v.value).join(';');
    this.displayDialog = true;
  }

  save(): void {
    if (!this.property.propertyName) {
      this.notify.warn(this.l('PropertyNameRequired'));
      return;
    }

    this.property.values = this.parseValues(this.valuesText);
    this.saving = true;

    const operation = this.property.id
      ? this._dynamicEntityPropertyService.updateDynamicProperty(this.property)
      : this._dynamicEntityPropertyService.createDynamicProperty(this.property);

    operation
      .pipe(finalize(() => (this.saving = false)))
      .subscribe(() => {
        this.notify.success(this.l('SavedSuccessfully'));
        this.displayDialog = false;
        this.loadProperties();
      });
  }

  remove(item: DynamicPropertyDto): void {
    this.message.confirm(this.l('DynamicPropertyDeleteWarningMessage', item.propertyName), this.l('AreYouSure'), isConfirmed => {
      if (!isConfirmed || !item.id) {
        return;
      }

      this._dynamicEntityPropertyService.deleteDynamicProperty(item.id).subscribe(() => {
        this.notify.success(this.l('SuccessfullyDeleted'));
        this.loadProperties();
      });
    });
  }

  private parseValues(text: string): DynamicPropertyValueDto[] {
    if (!text) {
      return [];
    }

    return text
      .split(';')
      .map(v => v.trim())
      .filter(v => v.length > 0)
      .map(v => {
        const value = new DynamicPropertyValueDto();
        value.value = v;
        return value;
      });
  }
}
