import { ChangeDetectionStrategy, Component, Injector, OnInit, forwardRef } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { FlatPermissionWithLevelDto, PermissionServiceProxy } from '@shared/service-proxies/service-proxies';

import { ControlValueAccessor, UntypedFormControl, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  standalone: false,
  selector: 'permission-combo',
  template: `<select class="form-control" [formControl]="selectedPermission">
    <option value="">{{ 'SearchWithThreeDot' | localize }}</option>
    <option *ngFor="let permission of permissions" [value]="permission.name">{{ permission.displayName }}</option>
  </select>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PermissionComboComponent),
      multi: true,
    },
  ],
})
export class PermissionComboComponent extends AppComponentBase implements OnInit, ControlValueAccessor {
  permissions: FlatPermissionWithLevelDto[] = [];
  selectedPermission = new UntypedFormControl('');

  onTouched: any = () => {};

  constructor(
    private readonly _permissionService: PermissionServiceProxy,
    injector: Injector,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._permissionService.getAllPermissions().subscribe(result => {
      result.items.forEach(item => {
        item.displayName = new Array(item.level + 1).join('   ') + ' ' + item.displayName;
      });

      this.permissions = result.items;
    });
  }

  writeValue(obj: any): void {
    if (this.selectedPermission) {
      this.selectedPermission.setValue(obj);
    }
  }

  registerOnChange(fn: any): void {
    this.selectedPermission.valueChanges.subscribe(fn);
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void { //NOSONAR ControlValueAccessor interface requires a boolean flag
    if (isDisabled) { //NOSONAR ControlValueAccessor interface requires a boolean flag
      this.selectedPermission.disable();
    } else {
      this.selectedPermission.enable();
    }
  }
}
