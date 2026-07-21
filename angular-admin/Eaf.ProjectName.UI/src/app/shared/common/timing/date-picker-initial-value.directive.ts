import { AfterViewInit, Directive, ElementRef, Injector, Input } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';

@Directive({
  standalone: false,
  selector: '[datePickerInitialValue]',
})
export class DatePickerInitialValueSetterDirective extends AppComponentBase implements AfterViewInit {
  hostElement: ElementRef;
  @Input() ngModel;

  constructor(
    injector: Injector,
    private readonly _element: ElementRef,
  ) {
    super(injector);
    this.hostElement = _element;
  }

  ngAfterViewInit(): void {
    if (this.ngModel) {
      setTimeout(() => {
        (this.hostElement.nativeElement as any).value = this.ngModel.format('L');
      });
    }
  }
}
