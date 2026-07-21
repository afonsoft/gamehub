import { Directive, AfterViewInit, ElementRef } from '@angular/core';

@Directive({
  standalone: false,
  selector: '[mMenuHorizontalOffcanvas]',
})
export class MenuHorizontalOffcanvasDirective implements AfterViewInit {
  menuOffcanvas: any;

  constructor(private readonly el: ElementRef) {}

  ngAfterViewInit(): void {
    // init the mOffcanvas plugin
    this.menuOffcanvas = new (mOffcanvas as any)(this.el.nativeElement, {
      overlay: true,
      baseClass: 'm-aside-header-menu-mobile',
      toggleBy: {
        target: 'm_aside_header_menu_mobile_toggle',
        state: 'm-brand__toggler--active',
      },
    });
  }
}
