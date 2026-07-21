import { Component, Injector, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AuditLogListDto } from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';
import { ModalDirective } from 'ngx-bootstrap/modal';

@Component({
  standalone: false,
  selector: 'auditLogDetailModal',
  templateUrl: './audit-log-detail-modal.component.html',
})
export class AuditLogDetailModalComponent extends AppComponentBase {
  @ViewChild('auditLogDetailModal', { static: true }) modal: ModalDirective;

  active = false;
  auditLog: AuditLogListDto;

  constructor(injector: Injector) {
    super(injector);
  }

  getExecutionTime(): string {

    return moment(this.auditLog.executionTime).fromNow() + ' (' + moment(this.auditLog.executionTime).format('YYYY-MM-DD HH:mm:ss') + ')';
  }

  getDurationAsMs(): string {

    return this.l('Xms', this.auditLog.executionDuration);
  }

  getFormattedParameters(): string {

    try {
      const json = JSON.parse(this.auditLog.parameters);
      return JSON.stringify(json, null, 4);
    } catch (e) {
      eaf.log.warn(e);
      return this.auditLog.parameters;
    }
  }

  show(record: AuditLogListDto): void {

    this.active = true;
    this.auditLog = record;

    this.modal.show();
  }

  close(): void {
    this.active = false;
    this.modal.hide();
  }
}
