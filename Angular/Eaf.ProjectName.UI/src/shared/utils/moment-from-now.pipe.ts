import { Pipe, PipeTransform } from '@angular/core';
import * as moment from 'moment';

@Pipe({ standalone: false, name: 'momentFromNow' })
export class MomentFromNowPipe implements PipeTransform {
  transform(value: moment.MomentInput) {
    if (!value) {
      return '';
    }

    return moment(value).fromNow();
  }
}
