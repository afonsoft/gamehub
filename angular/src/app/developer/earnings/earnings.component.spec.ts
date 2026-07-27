import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { DeveloperEarningsComponent } from './earnings.component';
import { DeveloperService } from '../../core/services/developer.service';
import { ErrorMapperService } from '../../core/services/error-mapper.service';
import { ButtonComponent } from '../../shared/ui/button/button.component';

describe('DeveloperEarningsComponent', () => {
  let fixture: ComponentFixture<DeveloperEarningsComponent>;
  let component: DeveloperEarningsComponent;
  let developerService: jasmine.SpyObj<DeveloperService>;

  beforeEach(async () => {
    developerService = jasmine.createSpyObj<DeveloperService>('DeveloperService', ['getEarnings', 'exportEarningsCsv']);
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
      imports: [DeveloperEarningsComponent, ButtonComponent],
      providers: [
        { provide: DeveloperService, useValue: developerService },
        ErrorMapperService,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DeveloperEarningsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads the earnings summary', () => {
    expect(component.earnings()?.totalDeveloperEstimatedRevenue).toBe(7);
    expect(component.state().loading).toBeFalse();
  });

  it('validates an inverted date range before requesting data', () => {
    component.from.set('2026-02-01');
    component.to.set('2026-01-01');

    component.applyFilter();

    expect(component.state().error?.message).toBe('The start date cannot be after the end date.');
    expect(component.state().error?.retryable).toBeFalse();
    expect(developerService.getEarnings).toHaveBeenCalledTimes(1);
  });

  it('shows a retryable error when loading fails', () => {
    const error = new HttpErrorResponse({ status: 500, statusText: 'Internal Server Error' });
    developerService.getEarnings.and.returnValue(throwError(() => error));

    component.loadEarnings();

    expect(component.state().error?.message).toBe('An unexpected error occurred. Please try again later.');
    expect(component.state().error?.retryable).toBeTrue();
    expect(component.state().error?.code).toBe('temporarily_unavailable');
    expect(component.state().loading).toBeFalse();
  });
});
