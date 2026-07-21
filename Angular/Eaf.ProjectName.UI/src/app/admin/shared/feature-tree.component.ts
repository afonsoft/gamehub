import { Component, Injector } from '@angular/core';
import { FeatureTreeEditModel } from '@app/admin/shared/feature-tree-edit.model';
import { AppComponentBase } from '@shared/common/app-component-base';
import { FlatFeatureDto, NameValueDto } from '@shared/service-proxies/service-proxies';
import { ArrayToTreeConverterService } from '@shared/utils/array-to-tree-converter.service';
import { TreeDataHelperService } from '@shared/utils/tree-data-helper.service';

import { TreeNode } from 'primeng/api';

@Component({
  standalone: false,
  selector: 'feature-tree',
  templateUrl: './feature-tree.component.html',
})
export class FeatureTreeComponent extends AppComponentBase {
  _editData: FeatureTreeEditModel;

  set editData(val: FeatureTreeEditModel) {
    this._editData = val;
    this.setTreeData(val.features);
    this.setSelectedNodes(val);
  }

  treeData: any;
  selectedFeatures: TreeNode[] = [];

  constructor(
    private readonly _arrayToTreeConverterService: ArrayToTreeConverterService,
    private readonly _treeDataHelperService: TreeDataHelperService,
    injector: Injector,
  ) {
    super(injector);
  }

  setTreeData(permissions: FlatFeatureDto[]) {
    this.treeData = this._arrayToTreeConverterService.createTree(permissions, 'parentName', 'name', null, 'children', [
      {
        target: 'label',
        source: 'displayName',
      },
      {
        target: 'expandedIcon',
        value: 'fa fa-folder-open m--font-warning',
      },
      {
        target: 'collapsedIcon',
        value: 'fa fa-folder m--font-warning',
      },
      {
        target: 'expanded',
        value: true,
      },
      {
        target: 'selectable',
        targetFunction(item) {
          return item.inputType.name === 'CHECKBOX';
        },
      },
    ]);
  }

  setSelectedNodes(val: FeatureTreeEditModel) {
    val.features?.forEach(feature => {
      const items = val.featureValues?.filter(f => f.name === feature.name) || [];
      if (items?.length === 1) {
        const item = items[0];
        this.setSelectedNode(item.name, item.value);
      } else {
        this.setSelectedNode(feature.name, feature.defaultValue);
      }
    });
  }

  setSelectedNode(featureName, value) {
    let node;

    if (value === 'true') {
      node = this._treeDataHelperService.findNode(this.treeData, { data: { name: featureName } });
      this.selectedFeatures.push(node);
    } else if (value && value !== 'false') {
      node = this._treeDataHelperService.findNode(this.treeData, { data: { name: featureName } });
      node.value = value;
      this.selectedFeatures.push(node);
    }
  }

  getGrantedFeatures(): NameValueDto[] {
    if (!this._editData.features) {
      return [];
    }

    const features: NameValueDto[] = [];

    for (const f of this._editData.features) {
      const feature = new NameValueDto();
      feature.name = f.name;
      feature.value = this.getFeatureValueByName(feature.name);

      features.push(feature);
    }

    return features;
  }

  onDropdownChange(node) {
    if (node.value) {
      node.selected = true;
    }
  }

  findFeatureByName(featureName: string): FlatFeatureDto {


    const feature = this._editData.features?.find(f => f.name === featureName);

    if (!feature) {
      eaf.log.warn('Could not find a feature by name: ' + featureName);
    }

    return feature;
  }

  findFeatureValueByName(featureName: string) {

    const feature = this.findFeatureByName(featureName);
    if (!feature) {
      return '';
    }

    const featureValue = this._editData.featureValues?.find(f => f.name === featureName);
    if (!featureValue) {
      return feature.defaultValue;
    }

    return featureValue.value;
  }

  isFeatureValueValid(featureName: string, value: string): boolean {
    const feature = this.findFeatureByName(featureName);
    if (!feature?.inputType?.validator) {
      return true;
    }

    const validator = feature.inputType.validator as any;
    if (validator.name === 'STRING') {
      return this.validateStringValue(validator, value);
    }

    if (validator.name === 'NUMERIC') {
      return this.validateNumericValue(validator, value);
    }

    return true;
  }

  private validateStringValue(validator: any, value: string): boolean {
    if (value === undefined || value === null) {
      return validator.allowNull;
    }

    if (typeof value !== 'string') {
      return false;
    }

    if (validator.minLength > 0 && value.length < validator.minLength) {
      return false;
    }

    if (validator.maxLength > 0 && value.length > validator.maxLength) {
      return false;
    }

    if (validator.regularExpression) {
      return new RegExp(validator.regularExpression).test(value);
    }

    return true;
  }

  private validateNumericValue(validator: any, value: string): boolean {
    const numValue = Number.parseInt(value);

    if (Number.isNaN(numValue)) {
      return false;
    }

    if (validator.minValue > numValue) {
      return false;
    }

    if (validator.maxValue > 0 && numValue > validator.maxValue) {
      return false;
    }

    return true;
  }

  areAllValuesValid(): boolean {
    let result = true;

    for (const feature of this._editData.features || []) {
      const value = this.getFeatureValueByName(feature.name);
      if (!this.isFeatureValueValid(feature.name, value)) {
        result = false;
      }
    }

    return result;
  }

  setFeatureValueByName(featureName: string, value: string): void {
    const featureValue = this._editData.featureValues?.find(f => f.name === featureName);
    if (!featureValue) {
      return;
    }

    featureValue.value = value;
  }

  isFeatureSelected(name: string): boolean {
    // let nodes = _.filter(this.selectedFeatures, { data: { name: name } });
    const nodes = this.selectedFeatures?.filter(o => o.data.name == name) || [];
    return nodes?.length === 1;
  }

  getFeatureValueByName(featureName: string): string {
    const feature = this._treeDataHelperService.findNode(this.treeData, { data: { name: featureName } });
    if (!feature) {
      return null;
    }

    if (feature.value) {
      return feature.value;
    }

    if (!this.isFeatureSelected(featureName)) {
      return 'false';
    }

    return 'true';
  }

  isFeatureEnabled(featureName: string): boolean {

    const value = this.findFeatureValueByName(featureName);
    return value.toLowerCase() === 'true';
  }

  nodeSelect(event) {
    let parentNode = this._treeDataHelperService.findParent(this.treeData, { data: { name: event.node.data.name } });

    while (parentNode != null) {
      this.selectedFeatures.push(parentNode);
      parentNode = this._treeDataHelperService.findParent(this.treeData, { data: { name: parentNode.data.name } });
    }
  }
}