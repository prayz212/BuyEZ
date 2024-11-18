import { Component, Input } from '@angular/core';
import { Product } from '../../models/product.model';

@Component({
  selector: 'product-item',
  templateUrl: './product-item.component.html',
})
export class ProductItemComponent {
  @Input() smallSize: boolean = false;
  @Input() showProductType: boolean = true;

  @Input() product!: Product;
}
