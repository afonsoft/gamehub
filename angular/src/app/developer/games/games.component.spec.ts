import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { DeveloperGamesComponent } from './games.component';
import { DeveloperService } from '../../core/services/developer.service';

describe('DeveloperGamesComponent', () => {
  let fixture: ComponentFixture<DeveloperGamesComponent>;
  let component: DeveloperGamesComponent;
  let developerService: jasmine.SpyObj<DeveloperService>;

  beforeEach(async () => {
    developerService = jasmine.createSpyObj<DeveloperService>('DeveloperService', [
      'getMyGames',
      'submitForReview',
    ]);
    developerService.getMyGames.and.returnValue(of({
      totalCount: 1,
      items: [{
        id: 'game-1',
        title: 'Test game',
        slug: 'test-game',
        status: 'Draft',
        latestBuildStatus: 'Approved',
        lastUpdated: '2026-01-01T00:00:00Z',
      }],
    }));
    developerService.submitForReview.and.returnValue(of({}));

    await TestBed.configureTestingModule({
      imports: [DeveloperGamesComponent],
      providers: [{ provide: DeveloperService, useValue: developerService }],
    }).compileComponents();

    fixture = TestBed.createComponent(DeveloperGamesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads games and exposes the filtered collection', () => {
    expect(component.games.length).toBe(1);
    expect(component.filteredGames[0].title).toBe('Test game');
  });

  it('shows a retryable error when loading fails', () => {
    developerService.getMyGames.and.returnValue(throwError(() => new Error('network')));

    component.loadGames();

    expect(component.errorMessage).toBe('Unable to load your games. Try again.');
    expect(component.loading).toBeFalse();
  });
});
