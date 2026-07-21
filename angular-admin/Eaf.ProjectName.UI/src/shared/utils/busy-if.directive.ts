import { Directive, ElementRef, Input, OnChanges, SimpleChanges } from '@angular/core';

@Directive({
  standalone: false,
  selector: '[busyIf]',
})
export class BusyIfDirective implements OnChanges {
  ngOnChanges(changes: SimpleChanges): void {
    if (changes.busyIf) {
      this.refreshState(changes.busyIf.currentValue);
    }
  }

  @Input() set busyIf(isBusy: boolean) {
    this.refreshState(isBusy);
  }

  constructor(private readonly _element: ElementRef) {}

  refreshState(isBusy: boolean): void {
    if (isBusy === undefined) {
      return;
    }

    if (isBusy) {
      eaf.ui.setBusy(this._element.nativeElement);
    } else {
      eaf.ui.clearBusy(this._element.nativeElement);
    }
  }
}
