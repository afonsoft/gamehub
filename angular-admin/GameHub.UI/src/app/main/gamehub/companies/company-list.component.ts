import { Component, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { CompanyDto, CompanyService } from './company.service';

@Component({
  standalone: false,
  selector: 'gamehub-company-list',
  templateUrl: './company-list.component.html',
  animations: [appModuleAnimation()],
})
export class CompanyListComponent implements OnInit {
  companies: CompanyDto[] = [];
  loading = false;
  totalCount = 0;

  constructor(private readonly companyService: CompanyService) {}

  ngOnInit(): void {
    this.loadCompanies();
  }

  loadCompanies(event?: any): void {
    const skipCount = event?.first || 0;
    const maxResultCount = event?.rows || 25;
    this.loading = true;
    this.companyService.getAll(skipCount, maxResultCount, 'name').subscribe({
      next: result => {
        this.companies = result?.items || [];
        this.totalCount = result?.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  deleteCompany(company: CompanyDto): void {
    if (!confirm(`Delete company "${company.name}"?`)) {
      return;
    }
    this.companyService.delete(company.id).subscribe(() => {
      this.companies = this.companies.filter(c => c.id !== company.id);
    });
  }
}
