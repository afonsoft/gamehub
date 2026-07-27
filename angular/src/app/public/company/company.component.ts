import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CompanyService, PublicCompanyDto } from '../../core/services/company.service';
import { AuthService } from '../../core/auth/auth.service';

interface JoinForm {
  userName: string;
  name: string;
  surname: string;
  emailAddress: string;
  password: string;
  role: string;
}

@Component({
  selector: 'app-company',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './company.component.html',
  styleUrl: './company.component.css',
})
export class CompanyComponent implements OnInit {
  private readonly companyService = inject(CompanyService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);

  company: PublicCompanyDto | null = null;
  loading = false;
  error = '';
  success = '';
  showJoinForm = false;
  joinModel: JoinForm = {
    userName: '',
    name: '',
    surname: '',
    emailAddress: '',
    password: '',
    role: 'Developer',
  };
  saving = false;

  ngOnInit(): void {
    const tenancyName = this.route.snapshot.paramMap.get('tenancyName');
    if (tenancyName) {
      this.loadCompany(tenancyName);
      this.joinModel.userName = this.route.snapshot.queryParamMap.get('userName') ?? '';
    }
  }

  loadCompany(tenancyName: string): void {
    this.loading = true;
    this.companyService.getByTenancyName(tenancyName).subscribe({
      next: company => {
        this.company = company;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Company not found.';
      },
    });
  }

  join(): void {
    if (!this.company) {
      return;
    }

    this.error = '';
    this.success = '';

    if (!this.joinModel.userName || !this.joinModel.name || !this.joinModel.surname || !this.joinModel.emailAddress || !this.joinModel.password) {
      this.error = 'Please fill in all required fields.';
      return;
    }

    this.saving = true;
    this.companyService.registerAndJoin({
      tenancyName: this.company.tenancyName,
      ...this.joinModel,
    }).subscribe({
      next: () => {
        this.saving = false;
        this.success = 'Account created and linked to the company. Please log in.';
        this.showJoinForm = false;
        void this.router.navigate(['/login'], {
          queryParams: { returnUrl: `/company/${this.company?.tenancyName}` },
        });
      },
      error: err => {
        this.saving = false;
        this.error = err?.error?.message || 'Could not join company. Please try again.';
      },
    });
  }
}
