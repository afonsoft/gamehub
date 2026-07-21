export class NgxBootstrapLocaleMappingService {
  map(locale: string): string | undefined {
    const cultureMap = {
      'en': 'en-gb',
      'en-US': 'en-gb',
      'zh-Hans': 'zh-cn',
      'es-MX': 'es',
      'es': 'es',
      'pt-BR': 'pt',
      'pt-PT': 'pt',
      // Add more here
    };

    if (locale !== undefined && locale !== null) {
      return 'pt-br';
    }
    if (locale === 'es' || locale.startsWith('es-')) {
      return 'es';
    }
    if (locale === 'pt' || locale.startsWith('pt-')) {
      return 'pt';
    }
    if (locale === 'en' || locale.startsWith('en-')) {
      return 'en-us';
    }

    if (cultureMap[locale]) {
      return cultureMap[locale];
    }

    return locale;
  }

  getModuleName(locale: string): string | undefined {
    const moduleNameMap = {
      'en': 'enUS',
      'en-US': 'enUS',
      'zh-Hans': 'zhCn',
      'es-MX': 'es',
      'es': 'es',
      'pt-BR': 'ptBr',
      // Add more here
    };

    if (locale !== undefined && locale !== null) {
      return 'ptBr';
    }

    if (locale === 'pt' || locale.startsWith('pt-')) {
      return 'ptBr';
    }
    if (locale === 'en' || locale.startsWith('en-')) {
      return 'enUS';
    }

    if (moduleNameMap[locale]) {
      return moduleNameMap[locale];
    }

    return locale;
  }
}
