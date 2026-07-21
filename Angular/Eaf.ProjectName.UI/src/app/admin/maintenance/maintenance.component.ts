import { Component, Injector, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { CachingServiceProxy, EntityDtoOfString, WebLogServiceProxy } from '@shared/service-proxies/service-proxies';
import { FileDownloadService } from '@shared/utils/file-download.service';

import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  templateUrl: './maintenance.component.html',
  animations: [appModuleAnimation()],
})
export class MaintenanceComponent extends AppComponentBase implements OnInit {
  loading = false;
  caches: any = null;
  logs: any = '';

  constructor(
    injector: Injector,
    private readonly _cacheService: CachingServiceProxy,
    private readonly _webLogService: WebLogServiceProxy,
    private readonly _fileDownloadService: FileDownloadService,
  ) {
    super(injector);
  }

  getCaches(): void {

    this.loading = true;
    this._cacheService
      .getAllCaches()
      .pipe(
        finalize(() => {
          this.loading = false;
        }),
      )
      .subscribe(result => {
        this.caches = result.items;
      });
  }

  clearCache(cacheName): void {

    const input = new EntityDtoOfString();
    input.id = cacheName;

    this._cacheService.clearCache(input).subscribe(() => {
      this.notify.success(this.l('CacheSuccessfullyCleared'));
    });
  }

  clearAllCaches(): void {

    this._cacheService.clearAllCaches().subscribe(() => {
      this.notify.success(this.l('AllCachesSuccessfullyCleared'));
    });
  }

  getWebLogs(): void {

    this._webLogService.getLatestWebLogs().subscribe(result => {
      this.logs = result.latestWebLogLines;
      this.fixWebLogsPanelHeight();
    });
  }

  downloadWebLogs = () => {

    this._webLogService.downloadWebLogs().subscribe(result => {
      this._fileDownloadService.downloadTempFile(result);
    });
  };

  getLogClass(log: string): string {
    if (log.includes('DEBUG') || log.includes('[DBG]')) {
      return 'label label-inline label-dark';
    }

    if (log.includes('INFO') || log.includes('[INF]')) {
      return 'label label-inline label-info';
    }

    if (log.includes('WARN') || log.includes('[WRN]')) {
      return 'label label-inline label-warning';
    }

    if (log.includes('ERROR') || log.includes('[ERR]')) {
      return 'label label-inline label-danger';
    }

    if (log.includes('FATAL') || log.includes('[FAT]') || log.includes('[FTL]')) {
      return 'label label-inline label-danger';
    }

    return '';
  }

  getLogType(log: string): string {
    if (log.includes('DEBUG') || log.includes('[DBG]')) {
      return 'DEBUG';
    }

    if (log.includes('INFO') || log.includes('[INF]')) {
      return 'INFO';
    }

    if (log.includes('WARN') || log.includes('[WRN]')) {
      return 'WARN';
    }

    if (log.includes('ERROR') || log.includes('[ERR]')) {
      return 'ERROR';
    }

    if (log.includes('FATAL') || log.includes('[FAT]') || log.includes('[FTL]')) {
      return 'FATAL';
    }

    return '';
  }

  getRawLogContent(log: string): string {
    const length = 50;
    return log.substring(0, Math.min(length, log.length));
  }

  fixWebLogsPanelHeight(): void {
    const panel = document.getElementsByClassName('full-height')[0];
    const windowHeight = document.body.clientHeight;
    const panelHeight = panel.clientHeight;
    const difference = windowHeight - panelHeight;
    const fixedHeight = panelHeight + difference;
    (panel as any).style.height = fixedHeight - 420 + 'px';
  }

  onResize(event): void {
    this.fixWebLogsPanelHeight();
  }

  ngOnInit(): void {

    this.getCaches();
    this.getWebLogs();
  }
}
