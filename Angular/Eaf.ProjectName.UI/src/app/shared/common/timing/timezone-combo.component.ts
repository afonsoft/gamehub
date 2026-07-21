import { ChangeDetectionStrategy, Component, Injector, Input, OnInit, forwardRef } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { NameValueDto, TimingServiceProxy, SettingScopes } from '@shared/service-proxies/service-proxies';
import { ControlValueAccessor, UntypedFormControl, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  standalone: false,
  selector: 'timezone-combo',
  template: ` <select class="form-control" [formControl]="selectedTimeZone">
    <option *ngFor="let timeZone of timeZones" [value]="timeZone.value">{{ timeZone.name }}</option>
  </select>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TimeZoneComboComponent),
      multi: true,
    },
  ],
})
export class TimeZoneComboComponent extends AppComponentBase implements OnInit, ControlValueAccessor {
  @Input() defaultTimezoneScope: SettingScopes;

  timeZones: NameValueDto[] = [];
  selectedTimeZone = new UntypedFormControl('');

  onTouched: any = () => {};

  constructor(
    private readonly _timingService: TimingServiceProxy,
    injector: Injector,
  ) {
    super(injector);
  }

  ngOnInit(): void {

    this._timingService.getTimezones(this.defaultTimezoneScope).subscribe(result => {
      this.timeZones = result.items;
    });
  }

  writeValue(obj: any): void {
    if (this.selectedTimeZone) {
      this.selectedTimeZone.setValue(obj);
    }
  }

  registerOnChange(fn: any): void {
    this.selectedTimeZone.valueChanges.subscribe(fn);
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void { //NOSONAR ControlValueAccessor interface requires a boolean flag
    if (isDisabled) { //NOSONAR ControlValueAccessor interface requires a boolean flag
      this.selectedTimeZone.disable();
    } else {
      this.selectedTimeZone.enable();
    }
  }
}
