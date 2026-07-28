import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { CompanyDto, CompanyService, CreateOrUpdateCompanyInput } from './company.service';

@Component({
  standalone: false,
  selector: 'gamehub-company-edit',
  templateUrl: './company-edit.component.html',
  animations: [appModuleAnimation()],
})
export class CompanyEditComponent implements OnInit {
  company: CreateOrUpdateCompanyInput = {
    tenancyName: '',
    name: '',
    primaryContactEmail: '',
    country: '',
  };
  companyId: number | null = null;
  saving = false;
  loading = false;
  error = '';

  constructor(
    private readonly companyService: CompanyService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.companyId = Number(idParam);
      this.loadCompany(this.companyId);
    }
  }

  loadCompany(id: number): void {
    this.loading = true;
    this.companyService.get(id).subscribe({
      next: company => {
        this.company = {
          tenancyName: company.tenancyName,
          name: company.name,
          primaryContactEmail: company.primaryContactEmail,
          country: company.country,
        };
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Could not load company.';
      },
    });
  }

  save(): void {
    this.error = '';

    if (!this.company.tenancyName || !this.company.name || !this.company.primaryContactEmail) {
      this.error = 'Tenancy name, company name and contact email are required.';
      return;
    }

    this.saving = true;
    const request = this.companyId
      ? this.companyService.update(this.companyId, this.company)
      : this.companyService.create(this.company);

    request.subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/app/main/gamehub/companies']);
      },
      error: () => {
        this.saving = false;
        this.error = 'Could not save company.';
      },
    });
  }
}
