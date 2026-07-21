import { Directive, ElementRef, AfterViewInit } from '@angular/core';

@Directive({
  standalone: false,
  selector: '[mMenuAsideToggle]',
})
export class MenuAsideToggleDirective implements AfterViewInit {
  toggle: any;
  constructor(private readonly el: ElementRef) {}

  ngAfterViewInit(): void {
    this.toggle = new (mToggle as any)(this.el.nativeElement, {
      target: 'body',
      targetState: 'm-brand--minimize m-aside-left--minimize',
      togglerState: 'm-brand__toggler--active',
    });
  }
}
