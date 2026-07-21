export class DomHelper {
  static waitUntilElementIsReady(selector: string, callback: any, checkPeriod?: number): void {
    const selectors = selector.split(',');
    const elementCount = selectors.length;

    if (!checkPeriod) {
      checkPeriod = 100;
    }

    const checkExist = setInterval(() => {
      let foundElementCount = 0;
      for (const selector of selectors.map(s => s.trim())) {
        if (selector.startsWith('#')) {
          const idSelector = selector.replace('#', '');
          foundElementCount = foundElementCount + (document.getElementById(idSelector) ? 1 : 0);
        } else if (selector.startsWith('.')) {
          const classSelector = selector.replace('.', '');
          foundElementCount = foundElementCount + (document.getElementsByClassName(classSelector) ? 1 : 0);
        }
      }

      if (foundElementCount >= elementCount) {
        clearInterval(checkExist);
        callback();
      }
    }, checkPeriod);
  }

  static createElement(tag: string, attributes: any[]): any {
    const el = document.createElement(tag);
    for (const attribute of attributes) {
      el.setAttribute(attribute.key, attribute.value);
    }

    return el;
  }

  static getElementByAttributeValue(tag: string, attribute: string, value: string) {
    const els = document.getElementsByTagName(tag);
    if (!els) {
      return undefined;
    }

    for (const el of Array.from(els)) {
      if (el.getAttribute(attribute) === value) {
        return el;
      }
    }

    return undefined;
  }
}
