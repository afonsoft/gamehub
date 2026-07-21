import { Component, Injector, Input } from '@angular/core';
import { PermissionTreeEditModel } from '@app/admin/shared/permission-tree-edit.model';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ArrayToTreeConverterService } from '@shared/utils/array-to-tree-converter.service';
import { TreeDataHelperService } from '@shared/utils/tree-data-helper.service';
import { FlatPermissionDto } from '@shared/service-proxies/service-proxies';
import { TreeNode } from 'primeng/api';
import * as _ from 'lodash';

@Component({
  standalone: false,
  selector: 'permission-tree',
  templateUrl: './permission-tree.component.html',
})
export class PermissionTreeComponent extends AppComponentBase {
  @Input() singleSelect: boolean;
  @Input() disableCascade: boolean;

  set editData(val: PermissionTreeEditModel) {
    this.setTreeData(val.permissions);
    this.setSelectedNodes(val.grantedPermissionNames);
  }

  treeData: any;
  selectedPermissions: TreeNode[] = [];
  filter = '';

  constructor(
    private readonly _arrayToTreeConverterService: ArrayToTreeConverterService,
    private readonly _treeDataHelperService: TreeDataHelperService,
    injector: Injector,
  ) {
    super(injector);
  }

  setTreeData(permissions: FlatPermissionDto[]) {
    this.treeData = this._arrayToTreeConverterService.createTree(permissions, 'parentName', 'name', null, 'children', [
      {
        target: 'label',
        source: 'displayName',
      },
      {
        target: 'expandedIcon',
        value: 'fa fa-folder-open text-warning m--font-warning',
      },
      {
        target: 'collapsedIcon',
        value: 'fa fa-folder text-warning m--font-warning',
      },
      {
        target: 'expanded',
        value: true,
      },
    ]);
  }

  setSelectedNodes(grantedPermissionNames: string[]) {
    this.selectedPermissions = [];
    _.forEach(grantedPermissionNames, permission => {
      const item = this._treeDataHelperService.findNode(this.treeData, { data: { name: permission } });
      if (item) {
        this.selectedPermissions.push(item);
      }
    });
  }

  getGrantedPermissionNames(): string[] {
    if (!this.selectedPermissions?.length) {
      return [];
    }

    const permissionNames = [];

    for (const selectedPermission of this.selectedPermissions) {
      permissionNames.push(selectedPermission.data.name);
    }

    return permissionNames;
  }

  nodeSelect(event) {
    if (this.singleSelect) {
      this.selectedPermissions = [event.node];
      return;
    }

    if (this.disableCascade) {
      return;
    }

    let parentNode = this._treeDataHelperService.findParent(this.treeData, { data: { name: event.node.data.name } });

    while (parentNode != null) {
      this.selectedPermissions.push(parentNode);
      parentNode = this._treeDataHelperService.findParent(this.treeData, { data: { name: parentNode.data.name } });
    }
  }

  onNodeUnselect(event) {
    if (this.disableCascade) {
      return;
    }

    const childrenNodes = this._treeDataHelperService.findChildren(this.treeData, { data: { name: event.node.data.name } });
    childrenNodes.push(event.node.data.name);
    _.remove(this.selectedPermissions, x => childrenNodes.includes(x.data.name));
  }

  filterPermissions(event): void {
    this.filterPermission(this.treeData, this.filter);
  }

  filterPermission(nodes, filterText): any {
    _.forEach(nodes, node => {
      if (node.data.displayName.toLowerCase().includes(filterText.toLowerCase())) {
        node.styleClass = this.showParentNodes(node);
      } else {
        node.styleClass = 'hidden-tree-node';
      }

      if (node.children) {
        this.filterPermission(node.children, filterText);
      }
    });
  }

  showParentNodes(node): void {
    if (!node.parent) {
      return;
    }

    node.parent.styleClass = '';
    this.showParentNodes(node.parent);
  }
}
