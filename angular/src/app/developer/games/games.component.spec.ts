import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { DeveloperGamesComponent } from './games.component';
import { DeveloperService } from '../../core/services/developer.service';
import { ErrorMapperService } from '../../core/services/error-mapper.service';
import { I18nService } from '../../core/i18n/i18n.service';
import { ButtonComponent } from '../../shared/ui/button/button.component';
import { provideRouter } from '@angular/router';

const mockI18nService: I18nService = {
  currentLang$: of('pt-BR'),
  translate: (key: string) => key,
  setLanguage: () => Promise.resolve(),
  init: () => Promise.resolve(),
  getCurrentLang: () => 'pt-BR',
} as unknown as I18nService;

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
      imports: [DeveloperGamesComponent, ButtonComponent],
      providers: [
        { provide: DeveloperService, useValue: developerService },
        ErrorMapperService,
        { provide: I18nService, useValue: mockI18nService },
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DeveloperGamesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads games and exposes the filtered collection', () => {
    expect(component.state().games.length).toBe(1);
    expect(component.filteredGames()[0].title).toBe('Test game');
    expect(component.state().loading).toBeFalse();
    expect(component.state().empty).toBeFalse();
  });

  it('shows a retryable error when loading fails', () => {
    const error = new HttpErrorResponse({ status: 500, statusText: 'Internal Server Error' });
    developerService.getMyGames.and.returnValue(throwError(() => error));

    component.loadGames();

    expect(component.state().error?.message).toBe('An unexpected error occurred. Please try again later.');
    expect(component.state().error?.code).toBe('temporarily_unavailable');
    expect(component.state().error?.retryable).toBeTrue();
    expect(component.state().loading).toBeFalse();
  });
});
