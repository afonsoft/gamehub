import { ThemeHelper } from '@app/shared/layout/themes/ThemeHelper';
import { AppConsts } from '@shared/AppConsts';
import { StyleLoaderService } from '@shared/utils/style-loader.service';
import * as rtlDetect from 'rtl-detect';

export class DynamicResourcesHelper {
  static loadResources(callback: () => void): void {
    DynamicResourcesHelper.loadStyles().then(() => {
      callback();
    });
  }

  static loadStyles(): Promise<any> {
    const theme = ThemeHelper.getThemeColor();

    const isRtl = rtlDetect.isRtlLang(eaf.localization.currentLanguage.name);

    if (isRtl) {
      document.documentElement.setAttribute('dir', 'rtl');
    }

    const styleLoaderService = new StyleLoaderService();

    const styleUrls = [
      AppConsts.appBaseUrl + '/assets/common/styles/themes/' + theme + '/style.bundle.css',
      AppConsts.appBaseUrl + '/assets/common/styles/themes/' + theme + '/customize.css',
    ].concat(DynamicResourcesHelper.getAdditionalThemeAssets());

    styleLoaderService.loadArray(styleUrls);

    if (isRtl) {
      styleLoaderService.load(AppConsts.appBaseUrl + '/assets/common/styles/abp-zero-template-rtl.min.css');
    }

    return Promise.resolve(true);
  }

  static getAdditionalThemeAssets(): string[] {
    const assetContributor = ThemeHelper.getTheme();
    if (!assetContributor) {
      return [];
    }

    return [];
  }
}
