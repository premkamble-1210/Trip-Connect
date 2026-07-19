import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'texttail',
})
export class TexttailPipe implements PipeTransform {
  transform(value: string | null | undefined, maxLength = 7): string {
    if (!value) {
      return '';
    }

    if (value.length > maxLength) {
      return value.substring(0, maxLength) + '...';
    }
    return value;
  }
}
