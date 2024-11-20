import { Component, Input } from '@angular/core';

@Component({
  selector: 'products-filter',
  templateUrl: './filter.component.html',
})
export class FilterComponent {
  @Input() title: string = '';
  @Input() options: string[] = [];
}
