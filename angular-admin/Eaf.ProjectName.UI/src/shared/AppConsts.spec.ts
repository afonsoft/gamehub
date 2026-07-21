import { AppConsts } from './AppConsts';

describe('AppConsts', () => {
  it('should have default admin username', () => {
    expect(AppConsts.userManagement.defaultAdminUserName).toBe('admin');
  });

  it('should have localization source names', () => {
    expect(AppConsts.localization.defaultLocalizationSourceName).toBe('ProjectName');
    expect(AppConsts.localization.defaultLocalizationSourceNameEaf).toBe('EafCore');
    expect(AppConsts.localization.defaultLocalizationSourceNameAbp).toBe('Abp');
    expect(AppConsts.localization.defaultLocalizationSourceNameAbpWeb).toBe('AbpWeb');
    expect(AppConsts.localization.defaultLocalizationSourceNameAbpZero).toBe('AbpZero');
  });

  it('should have authorization config', () => {
    expect(AppConsts.authorization.encrptedAuthTokenName).toBe('enc_auth_token');
  });

  it('should have default grid page size', () => {
    expect(AppConsts.grid.defaultPageSize).toBe(30);
  });

  it('should have theme user config', () => {
    expect(AppConsts.themeUser.themeName).toBe('themeUser');
    expect(AppConsts.themeUser.typeTheme).toBe('TypeTheme');
  });

  it('should have locale currency mappings', () => {
    expect(AppConsts.LocaleCurrency).toBeDefined();
    expect(AppConsts.LocaleCurrency.length).toBeGreaterThan(0);
  });

  it('should have BRL locale', () => {
    const brl = AppConsts.LocaleCurrency.find(l => l.currencyCode === 'BRL');
    expect(brl).toBeDefined();
    expect(brl.locale).toBe('pt-BR');
  });

  it('should have USD locale', () => {
    const usd = AppConsts.LocaleCurrency.find(l => l.currencyCode === 'USD');
    expect(usd).toBeDefined();
  });

  it('should have tenancy name placeholder', () => {
    expect(AppConsts.tenancyNamePlaceHolderInUrl).toBe('{TENANCY_NAME}');
  });
});
