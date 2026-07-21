import { AfterViewInit, Directive, ElementRef } from '@angular/core';

@Directive({
  standalone: false,
  selector: '[autoFocus]',
})
export class AutoFocusDirective implements AfterViewInit {
  constructor(private readonly _element: ElementRef) {}

  ngAfterViewInit(): void {
    this._element.nativeElement.focus();
  }
}
