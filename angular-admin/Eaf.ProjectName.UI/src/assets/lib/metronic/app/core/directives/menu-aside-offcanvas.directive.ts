import { Directive, AfterViewInit, ElementRef } from '@angular/core';

@Directive({
  standalone: false,
  selector: '[mMenuAsideOffcanvas]',
})
export class MenuAsideOffcanvasDirective implements AfterViewInit {
  menuOffcanvas: any;

  constructor(private readonly el: ElementRef) {}

  ngAfterViewInit(): void {
    const offcanvasClass = mUtil.hasClass(this.el.nativeElement, 'm-aside-left--offcanvas-default')
      ? 'm-aside-left--offcanvas-default'
      : 'm-aside-left';

    this.menuOffcanvas = new (mOffcanvas as any)(this.el.nativeElement, {
      baseClass: offcanvasClass,
      overlay: true,
      toggleBy: {
        target: 'm_aside_left_offcanvas_toggle',
        state: 'm-brand__toggler--active',
      },
    });
  }
}
