export class ThemeHelper {
  public static getThemeColor(): string {
    const theme = eaf.setting.get('App.UiManagement.Theme');
    if (theme === 'default') {
      return eaf.setting.get(theme + '.' + 'App.UiManagement.ThemeColor');
    }

    return theme;
  }

  public static getTheme(): string {
    return eaf.setting.get('App.UiManagement.Theme');
  }
}
