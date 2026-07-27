import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { GameCard } from '../../../core/services/game-catalog.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-game-card',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './game-card.component.html',
  styleUrl: './game-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GameCardComponent {
  @Input({ required: true }) game!: GameCard;

  readonly maxCategories = 3;
}
