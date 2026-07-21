///<reference path="../../../eaf-web-resources/Eaf/Framework/scripts/eaf.d.ts"/>
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FeatureCheckerService {
  get(featureName: string): eaf.features.IFeature {
    return eaf.features.get(featureName);
  }

  getValue(featureName: string): string {
    return eaf.features.getValue(featureName);
  }

  isEnabled(featureName: string): boolean {
    return eaf.features.isEnabled(featureName);
  }
}
