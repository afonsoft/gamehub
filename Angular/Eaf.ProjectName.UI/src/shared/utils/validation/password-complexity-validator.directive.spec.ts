import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { PasswordComplexityValidator } from './password-complexity-validator.directive';

@Component({
  standalone: false,
  template: `
    <form>
      <input
        name="password"
        [(ngModel)]="password"
        [requireDigit]="requireDigit"
        [requireUppercase]="requireUppercase"
        [requireLowercase]="requireLowercase"
        [requireNonAlphanumeric]="requireNonAlphanumeric"
        [requiredLength]="requiredLength"
      />
    </form>
  `,
})
class TestHostComponent {
  password = '';
  requireDigit = false;
  requireUppercase = false;
  requireLowercase = false;
  requireNonAlphanumeric = false;
  requiredLength = 0;
}

describe('PasswordComplexityValidator', () => {
  let directive: PasswordComplexityValidator;

  beforeEach(() => {
    directive = new PasswordComplexityValidator();
  });

  describe('validate', () => {
    it('deve retornar null quando nenhuma regra é exigida', () => {
      directive.requireDigit = false;
      directive.requireUppercase = false;
      directive.requireLowercase = false;
      directive.requireNonAlphanumeric = false;
      directive.requiredLength = 0;

      const result = directive.validate({ value: 'abc' } as any);
      expect(result).toBeNull();
    });

    it('deve falhar quando requireDigit é true e senha não contém dígito', () => {
      directive.requireDigit = true;
      const result = directive.validate({ value: 'abcdef' } as any);
      expect(result).toEqual({ requireDigit: true });
    });

    it('deve passar quando requireDigit é true e senha contém dígito', () => {
      directive.requireDigit = true;
      const result = directive.validate({ value: 'abc123' } as any);
      expect(result).toBeNull();
    });

    it('deve falhar quando requireUppercase é true e senha não contém maiúscula', () => {
      directive.requireUppercase = true;
      const result = directive.validate({ value: 'abcdef' } as any);
      expect(result).toEqual({ requireUppercase: true });
    });

    it('deve passar quando requireUppercase é true e senha contém maiúscula', () => {
      directive.requireUppercase = true;
      const result = directive.validate({ value: 'Abcdef' } as any);
      expect(result).toBeNull();
    });

    it('deve falhar quando requireLowercase é true e senha não contém minúscula', () => {
      directive.requireLowercase = true;
      const result = directive.validate({ value: 'ABCDEF' } as any);
      expect(result).toEqual({ requireLowercase: true });
    });

    it('deve passar quando requireLowercase é true e senha contém minúscula', () => {
      directive.requireLowercase = true;
      const result = directive.validate({ value: 'ABCDEf' } as any);
      expect(result).toBeNull();
    });

    it('deve falhar quando requiredLength é maior que o tamanho da senha', () => {
      directive.requiredLength = 8;
      const result = directive.validate({ value: 'abc' } as any);
      expect(result).toEqual({ requiredLength: true });
    });

    it('deve passar quando requiredLength é atendido', () => {
      directive.requiredLength = 4;
      const result = directive.validate({ value: 'abcdef' } as any);
      expect(result).toBeNull();
    });

    it('deve falhar quando requireNonAlphanumeric é true e senha só tem alfanuméricos', () => {
      directive.requireNonAlphanumeric = true;
      const result = directive.validate({ value: 'Abc123' } as any);
      expect(result).toEqual({ requireNonAlphanumeric: true });
    });

    it('deve passar quando requireNonAlphanumeric é true e senha contém caractere especial', () => {
      directive.requireNonAlphanumeric = true;
      const result = directive.validate({ value: 'Abc123!' } as any);
      expect(result).toBeNull();
    });

    it('deve retornar múltiplos erros quando várias regras falham', () => {
      directive.requireDigit = true;
      directive.requireUppercase = true;
      directive.requireLowercase = true;
      directive.requiredLength = 10;
      directive.requireNonAlphanumeric = true;

      const result = directive.validate({ value: 'abc' } as any);
      expect(result.requireDigit).toBe(true);
      expect(result.requireUppercase).toBe(true);
      expect(result.requiredLength).toBe(true);
      expect(result.requireNonAlphanumeric).toBe(true);
    });

    it('deve retornar null quando senha é null ou vazia', () => {
      directive.requireDigit = true;
      directive.requireUppercase = true;

      const result = directive.validate({ value: '' } as any);
      expect(result).toBeNull();
    });

    it('deve retornar null quando valor do controle é null', () => {
      directive.requireDigit = true;
      const result = directive.validate({ value: null } as any);
      expect(result).toBeNull();
    });
  });

  describe('integração com template', () => {
    let fixture: ComponentFixture<TestHostComponent>;
    let component: TestHostComponent;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [FormsModule],
        declarations: [TestHostComponent, PasswordComplexityValidator],
      }).compileComponents();

      fixture = TestBed.createComponent(TestHostComponent);
      component = fixture.componentInstance;
      fixture.detectChanges();
      await fixture.whenStable();
    });

    it('deve criar o componente host', () => {
      expect(component).toBeTruthy();
    });
  });
});
