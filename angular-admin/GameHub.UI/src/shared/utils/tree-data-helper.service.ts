import { Injectable } from '@angular/core';
import * as _ from 'lodash';

@Injectable()
export class TreeDataHelperService {
  findNode(data, selector): any {
    const nodes = _.filter(data, selector);
    if (nodes?.length === 1) {
      return nodes[0];
    }

    let foundNode = null;

    _.forEach(data, d => {
      if (!foundNode) {
        foundNode = this.findNode(d.children, selector);
      }
    });

    return foundNode;
  }

  findParent(data, nodeSelector) {
    const node = this.findNode(data, nodeSelector);
    if (!node) {
      return null;
    }

    return node.parent;
  }

  findChildren(data, selector) {
    const traverseChildren = function (node) {
      let names = [];
      if (node.children) {
        _.forEach(node.children, c => {
          names.push(c.data.name);
          names = names.concat(traverseChildren(c));
        });
      }
      return names;
    };

    const foundNode = this.findNode(data, selector);
    if (foundNode) {
      return traverseChildren(foundNode);
    } else {
      return [];
    }
  }
}
