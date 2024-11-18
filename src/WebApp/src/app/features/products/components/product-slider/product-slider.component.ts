import { Component } from '@angular/core';
import { Product } from '../../models/product.model';
import { ProductType } from '../../types/product-type';

@Component({
  selector: 'product-slider',
  templateUrl: './product-slider.component.html',
})
export class ProductSliderComponent {
  product: Product = {
    id: '08b0368c-a038-40d5-8c52-0e49cef3368a',
    name: 'Summer BD Shirt',
    price: 19.99,
    productType: ProductType.Men_TShirt,
  };
}
