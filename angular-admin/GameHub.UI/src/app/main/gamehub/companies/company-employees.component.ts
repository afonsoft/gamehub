import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { CompanyDto, CompanyEmployeeDto, CompanyService, InviteEmployeeInput } from './company.service';

@Component({
  standalone: false,
  selector: 'gamehub-company-employees',
  templateUrl: './company-employees.component.html',
  animations: [appModuleAnimation()],
})
export class CompanyEmployeesComponent implements OnInit {
  company: CompanyDto | null = null;
  employees: CompanyEmployeeDto[] = [];
  loading = false;
  saving = false;
  error = '';
  inviteModel: { emailOrUserName: string; role: string; isDefault: boolean } = {
    emailOrUserName: '',
    role: 'Developer',
    isDefault: false,
  };

  constructor(
    private readonly companyService: CompanyService,
    private readonly route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadCompany(id);
    }
  }

  loadCompany(id: number): void {
    this.loading = true;
    this.companyService.get(id).subscribe({
      next: company => {
        this.company = company;
        this.loadEmployees();
      },
      error: () => {
        this.loading = false;
        this.error = 'Could not load company.';
      },
    });
  }

  loadEmployees(): void {
    if (!this.company) {
      return;
    }
    this.companyService.getEmployees(this.company.id).subscribe({
      next: employees => {
        this.employees = employees || [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Could not load employees.';
      },
    });
  }

  invite(): void {
    if (!this.company || !this.inviteModel.emailOrUserName) {
      this.error = 'Select a user or email to invite.';
      return;
    }

    this.saving = true;
    const input: InviteEmployeeInput = {
      tenantId: this.company.id,
      emailOrUserName: this.inviteModel.emailOrUserName,
      role: this.inviteModel.role,
      isDefault: this.inviteModel.isDefault,
    };

    this.companyService.invite(input).subscribe({
      next: () => {
        this.saving = false;
        this.inviteModel = { emailOrUserName: '', role: 'Developer', isDefault: false };
        this.loadEmployees();
      },
      error: () => {
        this.saving = false;
        this.error = 'Could not invite employee. Make sure the user is registered.';
      },
    });
  }

  remove(employee: CompanyEmployeeDto): void {
    if (!this.company || !confirm(`Remove ${employee.userName} from company?`)) {
      return;
    }
    this.companyService.remove({ tenantId: this.company.id, userId: employee.userId }).subscribe(() => {
      this.employees = this.employees.filter(e => e.userId !== employee.userId);
    });
  }

  setDefault(employee: CompanyEmployeeDto): void {
    if (!this.company) {
      return;
    }
    this.companyService.setDefault({ tenantId: this.company.id, userId: employee.userId }).subscribe(() => {
      this.employees.forEach(e => (e.isDefault = e.userId === employee.userId));
    });
  }
}
