import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="not-found">
      <h1>404</h1>
      <p>Página não encontrada.</p>
      <a routerLink="/">Voltar para home</a>
    </div>
  `,
  styles: [
    ':host { display: flex; align-items: center; justify-content: center; min-height: 60vh; }',
    '.not-found { text-align: center; }',
    'h1 { font-size: 4rem; margin: 0; }',
    'p { margin: 1rem 0; }',
    'a { color: #ff5e57; text-decoration: underline; cursor: pointer; }',
  ],
})
export class NotFoundComponent {}
