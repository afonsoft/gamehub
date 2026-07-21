import { Injectable } from '@angular/core';

@Injectable()
export class LogService {
  info(message: any, ...args: any[]): void {
    // eslint-disable-next-line no-console
    console.info(message, ...args);
  }

  warn(message: any, ...args: any[]): void {
    // eslint-disable-next-line no-console
    console.warn(message, ...args);
  }

  error(message: any, ...args: any[]): void {
    // eslint-disable-next-line no-console
    console.error(message, ...args);
  }

  debug(message: any, ...args: any[]): void {
    // eslint-disable-next-line no-console
    console.debug(message, ...args);
  }

  fatal(message: any, ...args: any[]): void {
    this.error(message, ...args);
  }
}
