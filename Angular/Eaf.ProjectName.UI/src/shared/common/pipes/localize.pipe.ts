import { Injector, Pipe, PipeTransform } from '@angular/core';
import { LocalizationService } from '@eaf/localization/localization.service';
import { AppConsts } from '@shared/AppConsts';

@Pipe({
  standalone: false,
  name: 'localize',
})
export class LocalizePipe implements PipeTransform {
  localizationSourceName = AppConsts.localization.defaultLocalizationSourceName;
  localizationSourceNameEaf = AppConsts.localization.defaultLocalizationSourceNameEaf;
  LocalizationSourceNameAbp = AppConsts.localization.defaultLocalizationSourceNameAbp;
  LocalizationSourceNameAbpWeb = AppConsts.localization.defaultLocalizationSourceNameAbpWeb;
  LocalizationSourceNameAbpZero = AppConsts.localization.defaultLocalizationSourceNameAbpZero;
  LocalizationSourceNameEafAzureActiveDirectory = AppConsts.localization.defaultLocalizationSourceNameEafAzureActiveDirectory;
  LocalizationSourceNameEafLdap = AppConsts.localization.defaultLocalizationSourceNameEafLdap;

  localization: LocalizationService;

  constructor(injector: Injector) {
    this.localization = injector.get(LocalizationService);
  }

  l(key: string, ...args: any[]): string {
    return this.ls(this.localizationSourceName, key, ...args);
  }

  ls(sourcename: string, key: string, ...args: any[]): string {
    let localizedText = this.localization.localize(key, this.localizationSourceName);

    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, this.localizationSourceNameEaf);
    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, this.LocalizationSourceNameAbp);
    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, this.LocalizationSourceNameAbpWeb);
    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, this.LocalizationSourceNameAbpZero);
    if (!localizedText || localizedText == key)
      localizedText = this.localization.localize(key, this.LocalizationSourceNameEafAzureActiveDirectory);
    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, this.LocalizationSourceNameEafLdap);
    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, sourcename);

    args.unshift(localizedText);

    return eaf.utils.formatString.apply(this, args);
  }

  transform(key: string, ...args: any[]): string {
    return this.l(key, args);
  }
}
