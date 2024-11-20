import { Component, Input } from '@angular/core';

@Component({
  selector: 'product-promotion',
  templateUrl: './promotion.component.html',
})
export class PromotionComponent {
  @Input() title: string = '';
  @Input() description: string = '';
}
