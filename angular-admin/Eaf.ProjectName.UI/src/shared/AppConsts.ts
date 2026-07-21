export class AppConsts {
  static readonly tenancyNamePlaceHolderInUrl = '{TENANCY_NAME}';
  static remoteServiceBaseUrl: string; // NOSONAR
  static remoteServiceBaseUrlFormat: string; // NOSONAR
  static appBaseUrl: string; // NOSONAR
  static appBaseHref: string; // NOSONAR
  static appBaseUrlFormat: string; // NOSONAR
  static recaptchaSiteKey: string; // NOSONAR
  static appActiveDirectoryEnabled = false; // NOSONAR
  static appLdapEnabled = false; // NOSONAR
  static googleAnalytics: string; // NOSONAR
  static googleTagManager: string; // NOSONAR

  static localeMappings: any = []; // NOSONAR

  static readonly userManagement = {
    defaultAdminUserName: 'admin',
  };

  static readonly localization = {
    defaultLocalizationSourceName: 'ProjectName',
    defaultLocalizationSourceNameEaf: 'EafCore',
    defaultLocalizationSourceNameAbp: 'Abp',
    defaultLocalizationSourceNameAbpWeb: 'AbpWeb',
    defaultLocalizationSourceNameAbpZero: 'AbpZero',
    defaultLocalizationSourceNameEafAzureActiveDirectory: 'EafAzureActiveDirectory',
    defaultLocalizationSourceNameEafLdap: 'EafLdap',
  };

  static readonly authorization = {
    encrptedAuthTokenName: 'enc_auth_token',
  };

  static readonly grid = {
    defaultPageSize: 30,
  };

  static readonly themeUser = {
    themeName: 'themeUser',
    typeTheme: 'TypeTheme',
  };

  static readonly expirationToken = {
    keyName: 'timeExpirationToken',
    typeTheme: 'int',
  };

  static readonly LocaleCurrency = [
    {
      currencyCode: 'BRL',
      initials: 'BRA',
      locale: 'pt-BR',
    },
    {
      currencyCode: 'USD',
      initials: 'USA',
      locale: 'en',
    },
    {
      currencyCode: 'ARS',
      initials: 'ARG',
      locale: 'es-AR',
    },
    {
      currencyCode: 'CLP',
      initials: 'CHI',
      locale: 'es-CL',
    },
    {
      currencyCode: 'BOB',
      initials: 'BOL',
      locale: 'es-BO',
    },
    {
      currencyCode: 'DOP',
      initials: 'DOM',
      locale: 'es-DO',
    },
    {
      currencyCode: 'USD',
      initials: 'PER',
      locale: 'es-PE',
    },
    {
      currencyCode: 'PYG',
      initials: 'PAR',
      locale: 'es-PY',
    },
    {
      currencyCode: 'USD',
      initials: 'ECU',
      locale: 'es-EC',
    },
    {
      currencyCode: 'USD',
      initials: 'URU',
      locale: 'es-UY',
    },
  ];
}
