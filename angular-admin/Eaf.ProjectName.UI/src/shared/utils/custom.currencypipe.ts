import { formatCurrency, getCurrencySymbol } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';

@Pipe({
  standalone: false,
  name: 'mycurrency',
})
export class CustomCurrencyPipe implements PipeTransform {
  transform(
    value: number,
    currencyCode: string = AppConsts.LocaleCurrency.find(l => l.locale == eaf.localization.currentLanguage.name).currencyCode,
    display: string | boolean = 'symbol',
    // digitsInfo: string = '3.2-2',
    digitsInfo = '1.2-2',
    locale: string = eaf.localization.currentLanguage.name,
  ): string | null {
    return formatCurrency(
      value,
      locale,
      getCurrencySymbol(currencyCode, 'wide', eaf.localization.currentLanguage.name),
      currencyCode,
      digitsInfo,
    );
  }
}
