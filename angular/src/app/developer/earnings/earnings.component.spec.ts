import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { DeveloperEarningsComponent } from './earnings.component';
import { DeveloperService } from '../../core/services/developer.service';

describe('DeveloperEarningsComponent', () => {
  let fixture: ComponentFixture<DeveloperEarningsComponent>;
  let component: DeveloperEarningsComponent;
  let developerService: jasmine.SpyObj<DeveloperService>;

  beforeEach(async () => {
    developerService = jasmine.createSpyObj<DeveloperService>('DeveloperService', ['getEarnings']);
    developerService.getEarnings.and.returnValue(of({
      from: '2026-01-01',
      to: '2026-01-31',
      totalGrossEstimatedRevenue: 10,
      totalDeveloperEstimatedRevenue: 7,
      totalPlatformEstimatedRevenue: 3,
      totalCommercialBreaks: 1,
      totalRewardedBreaks: 2,
      games: [],
    }));

    await TestBed.configureTestingModule({
      imports: [DeveloperEarningsComponent],
      providers: [{ provide: DeveloperService, useValue: developerService }],
    }).compileComponents();

    fixture = TestBed.createComponent(DeveloperEarningsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads the earnings summary', () => {
    expect(component.earnings?.totalDeveloperEstimatedRevenue).toBe(7);
  });

  it('validates an inverted date range before requesting data', () => {
    component.from = '2026-02-01';
    component.to = '2026-01-01';

    component.applyFilter();

    expect(component.errorMessage).toBe('The start date cannot be after the end date.');
    expect(developerService.getEarnings).toHaveBeenCalledTimes(1);
  });

  it('shows a retryable error when loading fails', () => {
    developerService.getEarnings.and.returnValue(throwError(() => new Error('network')));

    component.loadEarnings();

    expect(component.errorMessage).toBe('Unable to load earnings. Try again.');
    expect(component.loading).toBeFalse();
  });
});
