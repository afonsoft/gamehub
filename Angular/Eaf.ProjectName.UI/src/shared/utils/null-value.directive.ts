import { Directive, ElementRef, HostListener } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
  standalone: false,
  selector: 'input[nullValue]',
})
export class NullDefaultValueDirective {
  constructor(
    private readonly el: ElementRef,
    private readonly control: NgControl,
  ) {}

  @HostListener('input', ['$event.target'])
  onEvent(target: HTMLInputElement) {
    this.control.viewToModelUpdate(target.value === '' ? null : target.value);
  }
}
