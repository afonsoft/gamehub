///<reference path="../../../eaf-web-resources/Eaf/Framework/scripts/eaf.d.ts"/>

import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class EafSessionService {
  get userId(): number | undefined {
    return eaf.session.userId;
  }

  get tenantId(): number | undefined {
    return eaf.session.tenantId;
  }

  get impersonatorUserId(): number | undefined {
    return eaf.session.impersonatorUserId;
  }

  get impersonatorTenantId(): number | undefined {
    return eaf.session.impersonatorTenantId;
  }

  get multiTenancySide(): eaf.multiTenancy.sides {
    return eaf.session.multiTenancySide;
  }
}
